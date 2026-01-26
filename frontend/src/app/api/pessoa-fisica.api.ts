import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from './api.config';
import { PessoaFisica } from './api.models';

export interface CreatePessoaFisicaRequest {
  nome: string;
  cpf: string;
  cep: string;
  numero: string;
  complemento?: string | null;
}

@Injectable({ providedIn: 'root' })
export class PessoaFisicaApi {
  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<PessoaFisica[]>(`${API_BASE_URL}/api/pessoas-fisicas`);
  }

  create(req: CreatePessoaFisicaRequest) {
    // O backend espera CPF/CEP/número como string (aceita com pontuação, mas aqui normalizamos)
    const payload: CreatePessoaFisicaRequest = {
      ...req,
      cpf: (req.cpf ?? '').replace(/\D/g, ''),
      cep: (req.cep ?? '').replace(/\D/g, ''),
      numero: (req.numero ?? '').trim(),
      complemento: (req.complemento ?? '').trim() || null,
    };

    return this.http.post<PessoaFisica>(`${API_BASE_URL}/api/pessoas-fisicas`, payload);
  }
}
