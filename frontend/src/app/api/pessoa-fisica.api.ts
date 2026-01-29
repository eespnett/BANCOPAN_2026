import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { CreatePessoaFisicaRequest, PessoaFisica } from './api.models';

type ListResponse<T> = {
  correlationId?: string;
  items?: T[];
};

@Injectable({ providedIn: 'root' })
export class PessoaFisicaApi {
  constructor(private http: HttpClient) {}

  getAll(): Observable<PessoaFisica[]> {
    return this.http
      .get<ListResponse<PessoaFisica>>('/api/pessoas-fisicas')
      .pipe(map((r) => r?.items ?? []));
  }

  create(payload: CreatePessoaFisicaRequest) {
    // o backend retorna { id, message, correlationId } — não precisa tipar como PessoaFisica
    return this.http.post('/api/pessoas-fisicas', payload);
  }

  update(id: string, payload: Partial<CreatePessoaFisicaRequest>) {
    return this.http.put(`/api/pessoas-fisicas/${id}`, payload);
  }

  delete(id: string) {
    return this.http.delete(`/api/pessoas-fisicas/${id}`);
  }
}
