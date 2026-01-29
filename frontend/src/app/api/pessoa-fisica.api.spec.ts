import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { PessoaFisicaApi } from './pessoa-fisica.api';

describe('PessoaFisicaApi', () => {
  let api: PessoaFisicaApi;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PessoaFisicaApi, provideHttpClient(), provideHttpClientTesting()],
    });

    api = TestBed.inject(PessoaFisicaApi);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getAll should map items or return [] when items is missing', () => {
    let result1: any;
    api.getAll().subscribe((r) => (result1 = r));

    const req1 = httpMock.expectOne('/api/pessoas-fisicas');
    req1.flush({ correlationId: 'c1' }); // sem items
    expect(result1).toEqual([]);

    let result2: any;
    api.getAll().subscribe((r) => (result2 = r));

    const req2 = httpMock.expectOne('/api/pessoas-fisicas');
    req2.flush({ items: [{ id: '1', nome: 'A', cpf: '1', enderecoId: 'e', createdAtUtc: 'x' }] });
    expect(result2.length).toBe(1);
  });

  it('create should POST to /api/pessoas-fisicas', () => {
    api.create({ nome: 'A', cpf: '1', cep: '2', numero: '3' }).subscribe();

    const req = httpMock.expectOne('/api/pessoas-fisicas');
    expect(req.request.method).toBe('POST');
    req.flush({ id: 'ok' });
  });

  it('update should PUT to /api/pessoas-fisicas/:id', () => {
    api.update('123', { nome: 'B' }).subscribe();

    const req = httpMock.expectOne('/api/pessoas-fisicas/123');
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('delete should DELETE /api/pessoas-fisicas/:id', () => {
    api.delete('123').subscribe();

    const req = httpMock.expectOne('/api/pessoas-fisicas/123');
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });
});
