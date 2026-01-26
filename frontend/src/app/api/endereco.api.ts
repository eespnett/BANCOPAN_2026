import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Endereco } from './api.models';

@Injectable({ providedIn: 'root' })
export class EnderecoApi {
  constructor(private http: HttpClient) {}

  getByCep(cep: string) {
    const safe = (cep ?? '').replace(/\D/g, '');
    // IMPORTANTE:
    // No backend do CasePan, o endpoint GET /api/enderecos/{id} espera um GUID (ID do registro),
    // não um CEP. Como a tela "Buscar CEP" precisa resolver o CEP para exibir rua/bairro/cidade/UF,
    // consumimos o ViaCEP diretamente.
    //
    // O cadastro (PF/PJ) continua indo para o backend, que também valida o CEP e cria o Endereço
    // internamente.
    return this.http.get<Endereco>(`https://viacep.com.br/ws/${safe}/json/`);
  }
}
