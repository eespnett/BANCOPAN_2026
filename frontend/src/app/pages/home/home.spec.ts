import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { HomeComponent } from './home';
import { EnderecoApi } from '../../api/endereco.api';
import { PessoaFisicaApi } from '../../api/pessoa-fisica.api';
import { PessoaJuridicaApi } from '../../api/pessoa-juridica.api';
import type { Endereco, PessoaFisica, PessoaJuridica } from '../../api/api.models';

describe('HomeComponent', () => {
  let fixture: ComponentFixture<HomeComponent>;
  let component: HomeComponent;

  let enderecoApi: { getByCep: any };
  let pfApi: { getAll: any; create: any };
  let pjApi: { getAll: any; create: any };

  const endereco: Endereco = {
    cep: '01001000',
    logradouro: 'Praça da Sé',
    bairro: 'Sé',
    localidade: 'São Paulo',
    uf: 'SP',
    complemento: null,
  };

  const pf1: PessoaFisica = {
    id: 'pf-1',
    nome: 'Maria',
    cpf: '12345678901',
    enderecoId: 'end-1',
    createdAtUtc: new Date().toISOString(),
  };

  const pj1: PessoaJuridica = {
    id: 'pj-1',
    razaoSocial: 'Empresa X Ltda',
    cnpj: '12345678000199',
    enderecoId: 'end-2',
    createdAtUtc: new Date().toISOString(),
  };

  beforeEach(async () => {
    enderecoApi = { getByCep: vi.fn() };
    pfApi = { getAll: vi.fn(), create: vi.fn() };
    pjApi = { getAll: vi.fn(), create: vi.fn() };

    // padrão para não estourar ngOnInit
    pfApi.getAll.mockReturnValue(of([]));
    pjApi.getAll.mockReturnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [
        { provide: EnderecoApi, useValue: enderecoApi },
        { provide: PessoaFisicaApi, useValue: pfApi },
        { provide: PessoaJuridicaApi, useValue: pjApi },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.clearAllMocks();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit should load PF and PJ lists (success)', () => {
    pfApi.getAll.mockReturnValue(of([pf1]));
    pjApi.getAll.mockReturnValue(of([pj1]));

    fixture.detectChanges(); // dispara ngOnInit -> refreshAll

    expect(component.loading).toBe(false);
    expect(component.pessoasFisicas.length).toBe(1);
    expect(component.pessoasJuridicas.length).toBe(1);
  });

  it('refreshAll should show error notice when API fails', () => {
    pfApi.getAll.mockReturnValue(throwError(() => ({ error: { message: 'Falhou' } })));
    pjApi.getAll.mockReturnValue(of([]));

    fixture.detectChanges();

    expect(component.loading).toBe(false);
    expect(component.notice?.type).toBe('error');
    expect((component.notice?.text ?? '').length).toBeGreaterThan(0);
  });

  it('buscarEnderecoPF should block when CEP is invalid (8 digits) and autoclose notice', () => {
    vi.useFakeTimers();

    component.pfForm.get('cep')!.setValue('123'); // inválido
    component.buscarEnderecoPF();

    expect(enderecoApi.getByCep).not.toHaveBeenCalled();
    expect(component.notice?.type).toBe('error');

    vi.advanceTimersByTime(7000);
    expect(component.notice).toBeNull();
  });

  it('buscarEnderecoPF should call EnderecoApi with digits-only CEP and set preview', () => {
    enderecoApi.getByCep.mockReturnValue(of(endereco));

    component.pfForm.get('cep')!.setValue('01.001-000'); // com máscara
    component.buscarEnderecoPF();

    expect(enderecoApi.getByCep).toHaveBeenCalledWith('01001000');
    expect(component.pfCepInfo?.cep).toBe('01001000');
    expect(component.loadingCepPf).toBe(false);
    expect(component.notice?.type).toBe('info');
  });

  it('buscarEnderecoPJ should call EnderecoApi and set preview', () => {
    enderecoApi.getByCep.mockReturnValue(of(endereco));

    component.pjForm.get('cep')!.setValue('01001-000');
    component.buscarEnderecoPJ();

    expect(enderecoApi.getByCep).toHaveBeenCalledWith('01001000');
    expect(component.pjCepInfo?.cep).toBe('01001000');
    expect(component.loadingCepPj).toBe(false);
    expect(component.notice?.type).toBe('info');
  });

  it('criarPF should show validation error when form is invalid', () => {
    component.criarPF();

    expect(pfApi.create).not.toHaveBeenCalled();
    expect(component.notice?.type).toBe('error');
  });

  it('criarPF should sanitize cpf/cep/numero, call create, reload list and reset form', () => {
    pfApi.create.mockReturnValue(of({ id: 'pf-2' }));
    pfApi.getAll.mockReturnValue(of([pf1]));

    component.pfForm.patchValue({
      nome: '  Maria  ',
      cpf: '123.456.789-01',
      cep: '01.001-000',
      numero: '12A', // vira 12 por bindDigitsOnly
      complemento: '  Apt 1  ',
    });

    component.criarPF();

    expect(pfApi.create).toHaveBeenCalledTimes(1);
    const payloadSent = pfApi.create.mock.calls[0][0];

    expect(payloadSent).toEqual({
      nome: 'Maria',
      cpf: '12345678901',
      cep: '01001000',
      numero: '12',
      complemento: 'Apt 1',
    });

    expect(component.pessoasFisicas.length).toBe(1);
    expect(component.notice?.type).toBe('success');
    expect(component.pfCepInfo).toBeNull();
  });

  it('criarPJ should sanitize cnpj/cep/numero, call create and reload list', () => {
    pjApi.create.mockReturnValue(of({ id: 'pj-2' }));
    pjApi.getAll.mockReturnValue(of([pj1]));

    component.pjForm.patchValue({
      razaoSocial: '  Empresa X Ltda  ',
      cnpj: '12.345.678/0001-99',
      cep: '01001-000',
      numero: '100B', // vira 100
      complemento: '', // vira undefined
    });

    component.criarPJ();

    expect(pjApi.create).toHaveBeenCalledTimes(1);
    const payloadSent = pjApi.create.mock.calls[0][0];

    expect(payloadSent).toEqual({
      razaoSocial: 'Empresa X Ltda',
      cnpj: '12345678000199',
      cep: '01001000',
      numero: '100',
      complemento: undefined,
    });

    expect(component.pessoasJuridicas.length).toBe(1);
    expect(component.notice?.type).toBe('success');
    expect(component.pjCepInfo).toBeNull();
  });

  it('template: should render CEP preview when pfCepInfo exists', () => {
    component.pfCepInfo = endereco;
    fixture.detectChanges();

    const preview = fixture.nativeElement.querySelector('.cep-preview') as HTMLElement;
    expect(preview).toBeTruthy();
    expect(preview.textContent).toContain('Praça da Sé');
  });

  it('template: submit PF form should call criarPF()', () => {
    const spy = vi.spyOn(component, 'criarPF');
    fixture.detectChanges();

    const pfFormEl = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    pfFormEl.dispatchEvent(new Event('submit'));

    expect(spy).toHaveBeenCalled();
  });
});
