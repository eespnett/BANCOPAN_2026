import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { PessoaJuridicaApi } from './pessoa-juridica.api';

describe('PessoaJuridicaApi', () => {
  let api: PessoaJuridicaApi;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PessoaJuridicaApi, provideHttpClient(), provideHttpClientTesting()],
    });

    api = TestBed.inject(PessoaJuridicaApi);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getAll should map items or return [] when items is missing', () => {
    let result1: any;
    api.getAll().subscribe((r) => (result1 = r));

    const req1 = httpMock.expectOne('/api/pessoas-juridicas');
    req1.flush({ correlationId: 'c1' });
    expect(result1).toEqual([]);

    let result2: any;
    api.getAll().subscribe((r) => (result2 = r));

    const req2 = httpMock.expectOne('/api/pessoas-juridicas');
    req2.flush({
      items: [{ id: '1', razaoSocial: 'X', cnpj: '1', enderecoId: 'e', createdAtUtc: 'x' }],
    });
    expect(result2.length).toBe(1);
  });

  it('create should POST to /api/pessoas-juridicas', () => {
    api.create({ razaoSocial: 'X', cnpj: '1', cep: '2', numero: '3' }).subscribe();

    const req = httpMock.expectOne('/api/pessoas-juridicas');
    expect(req.request.method).toBe('POST');
    req.flush({ id: 'ok' });
  });

  it('update should PUT to /api/pessoas-juridicas/:id', () => {
    api.update('123', { razaoSocial: 'Y' }).subscribe();

    const req = httpMock.expectOne('/api/pessoas-juridicas/123');
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('delete should DELETE /api/pessoas-juridicas/:id', () => {
    api.delete('123').subscribe();

    const req = httpMock.expectOne('/api/pessoas-juridicas/123');
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });
});
