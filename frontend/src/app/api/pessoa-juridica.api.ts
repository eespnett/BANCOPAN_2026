import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { CreatePessoaJuridicaRequest, PessoaJuridica } from './api.models';

type ListResponse<T> = {
  correlationId?: string;
  items?: T[];
};

@Injectable({ providedIn: 'root' })
export class PessoaJuridicaApi {
  constructor(private http: HttpClient) {}

  getAll(): Observable<PessoaJuridica[]> {
    return this.http
      .get<ListResponse<PessoaJuridica>>('/api/pessoas-juridicas')
      .pipe(map((r) => r?.items ?? []));
  }

  create(payload: CreatePessoaJuridicaRequest) {
    return this.http.post('/api/pessoas-juridicas', payload);
  }

  update(id: string, payload: Partial<CreatePessoaJuridicaRequest>) {
    return this.http.put(`/api/pessoas-juridicas/${id}`, payload);
  }

  delete(id: string) {
    return this.http.delete(`/api/pessoas-juridicas/${id}`);
  }
}
