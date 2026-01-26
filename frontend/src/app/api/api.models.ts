export interface Endereco {
  id?: string;
  cep: string;
  logradouro: string;
  numero?: string;
  complemento?: string | null;
  bairro?: string | null;
  localidade?: string | null;
  uf?: string | null;
  createdAtUtc?: string;
  // ViaCEP retorna { erro: true } quando o CEP não existe
  erro?: boolean;
}

export interface PessoaFisica {
  id: string;
  nome: string;
  cpf: string;
  enderecoId: string;
  createdAtUtc: string;
}

export interface PessoaJuridica {
  id: string;
  razaoSocial: string;
  cnpj: string;
  enderecoId: string;
  createdAtUtc: string;
}

export interface CreatePessoaFisicaRequest {
  nome: string;
  cpf: string;
  cep: string;
  numero: string;
  complemento?: string | null;
}

export interface CreatePessoaJuridicaRequest {
  razaoSocial: string;
  cnpj: string;
  cep: string;
  numero: string;
  complemento?: string | null;
}
