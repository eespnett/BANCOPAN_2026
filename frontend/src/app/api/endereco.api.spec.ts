import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { EnderecoApi } from './endereco.api';

describe('EnderecoApi', () => {
  let api: EnderecoApi;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [EnderecoApi, provideHttpClient(), provideHttpClientTesting()],
    });

    api = TestBed.inject(EnderecoApi);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should sanitize CEP to digits-only in URL', () => {
    api.getByCep('01.001-000').subscribe();

    const req = httpMock.expectOne('https://viacep.com.br/ws/01001000/json/');
    expect(req.request.method).toBe('GET');
    req.flush({ cep: '01001000' });
  });

  it('should handle null/undefined CEP input (still makes request)', () => {
    api.getByCep(undefined as any).subscribe();

    const req = httpMock.expectOne('https://viacep.com.br/ws//json/');
    expect(req.request.method).toBe('GET');
    req.flush({ cep: '' });
  });
});
