import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from './api.config';
import { PessoaJuridica } from './api.models';

export interface CreatePessoaJuridicaRequest {
  razaoSocial: string;
  cnpj: string;
  cep: string;
  numero: string;
  complemento?: string | null;
}

@Injectable({ providedIn: 'root' })
export class PessoaJuridicaApi {
  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<PessoaJuridica[]>(`${API_BASE_URL}/api/pessoas-juridicas`);
  }

  create(req: CreatePessoaJuridicaRequest) {
    const payload: CreatePessoaJuridicaRequest = {
      ...req,
      cnpj: (req.cnpj ?? '').replace(/\D/g, ''),
      cep: (req.cep ?? '').replace(/\D/g, ''),
      numero: (req.numero ?? '').trim(),
      complemento: (req.complemento ?? '').trim() || null,
    };

    return this.http.post<PessoaJuridica>(`${API_BASE_URL}/api/pessoas-juridicas`, payload);
  }
}
