import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { finalize, forkJoin, switchMap } from 'rxjs';

import { EnderecoApi } from '../../api/endereco.api';
import { PessoaFisicaApi } from '../../api/pessoa-fisica.api';
import { PessoaJuridicaApi } from '../../api/pessoa-juridica.api';
import type {
  CreatePessoaFisicaRequest,
  CreatePessoaJuridicaRequest,
  Endereco,
  PessoaFisica,
  PessoaJuridica,
} from '../../api/api.models';

type NoticeType = 'success' | 'error' | 'info';
type Notice = { type: NoticeType; text: string };

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './home.html',
  styleUrls: ['./home.scss'],
})
export class HomeComponent implements OnInit {
  loading = false;
  loadingPf = false;
  loadingPj = false;
  loadingCepPf = false;
  loadingCepPj = false;

  // Mensagens (sucesso/erro/info)
  notice: Notice | null = null;
  private noticeTimer: any = null;

  // Preview do CEP (via backend)
  pfCepInfo: Endereco | null = null;
  pjCepInfo: Endereco | null = null;

  // Listas
  pessoasFisicas: PessoaFisica[] = [];
  pessoasJuridicas: PessoaJuridica[] = [];

  // Forms
  pfForm: FormGroup;
  pjForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private enderecoApi: EnderecoApi,
    private pfApi: PessoaFisicaApi,
    private pjApi: PessoaJuridicaApi
  ) {
    // Importante: criar forms no constructor (evita "fb used before initialization")
    this.pfForm = this.fb.group({
      nome: ['', [Validators.required, Validators.maxLength(120)]],
      cpf: ['', [Validators.required, Validators.minLength(11), Validators.maxLength(11)]],
      cep: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(8)]],
      numero: ['', [Validators.required, Validators.maxLength(20)]],
      complemento: ['', [Validators.maxLength(60)]],
    });

    this.pjForm = this.fb.group({
      razaoSocial: ['', [Validators.required, Validators.maxLength(160)]],
      cnpj: ['', [Validators.required, Validators.minLength(14), Validators.maxLength(14)]],
      cep: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(8)]],
      numero: ['', [Validators.required, Validators.maxLength(20)]],
      complemento: ['', [Validators.maxLength(60)]],
    });

    // Permite colar com máscara (pontos/traços) e normaliza automaticamente para dígitos
    this.bindDigitsOnly(this.pfForm, 'cpf', 11);
    this.bindDigitsOnly(this.pfForm, 'cep', 8);
    this.bindDigitsOnly(this.pfForm, 'numero', null);

    this.bindDigitsOnly(this.pjForm, 'cnpj', 14);
    this.bindDigitsOnly(this.pjForm, 'cep', 8);
    this.bindDigitsOnly(this.pjForm, 'numero', null);
  }

  ngOnInit(): void {
    this.refreshAll();
  }

  refreshAll(): void {
    this.loading = true;

    forkJoin({
      pf: this.pfApi.getAll(),
      pj: this.pjApi.getAll(),
    })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: ({ pf, pj }) => {
          this.pessoasFisicas = pf ?? [];
          this.pessoasJuridicas = pj ?? [];
        },
        error: (err) => {
          this.showNotice('error', this.getHttpError(err) || 'Falha ao carregar as listas.');
        },
      });
  }

  // ---------------------------
  // CEP (PF / PJ)
  // ---------------------------
  buscarEnderecoPF(): void {
    const cep = this.onlyDigits(this.pfForm.get('cep')?.value, 8);
    if (cep.length !== 8) {
      this.showNotice('error', 'Informe um CEP válido (8 dígitos).');
      return;
    }

    this.loadingCepPf = true;
    this.enderecoApi
      .getByCep(cep)
      .pipe(finalize(() => (this.loadingCepPf = false)))
      .subscribe({
        next: (end) => {
          this.pfCepInfo = end;
          this.showNotice('info', 'Endereço localizado. Preencha número/complemento e finalize o cadastro.');
        },
        error: (err) => {
          this.showNotice('error', this.getHttpError(err) || 'Não foi possível buscar o CEP.');
        },
      });
  }

  buscarEnderecoPJ(): void {
    const cep = this.onlyDigits(this.pjForm.get('cep')?.value, 8);
    if (cep.length !== 8) {
      this.showNotice('error', 'Informe um CEP válido (8 dígitos).');
      return;
    }

    this.loadingCepPj = true;
    this.enderecoApi
      .getByCep(cep)
      .pipe(finalize(() => (this.loadingCepPj = false)))
      .subscribe({
        next: (end) => {
          this.pjCepInfo = end;
          this.showNotice('info', 'Endereço localizado. Preencha número/complemento e finalize o cadastro.');
        },
        error: (err) => {
          this.showNotice('error', this.getHttpError(err) || 'Não foi possível buscar o CEP.');
        },
      });
  }

  // ---------------------------
  // Create PF / PJ
  // ---------------------------
  criarPF(): void {
    this.pfForm.markAllAsTouched();

    const cpf = this.onlyDigits(this.pfForm.get('cpf')?.value, 11);
    const cep = this.onlyDigits(this.pfForm.get('cep')?.value, 8);

    // garante consistência (mesmo que usuário cole com máscara)
    this.pfForm.patchValue({ cpf, cep }, { emitEvent: false });

    if (this.pfForm.invalid) {
      this.showNotice('error', 'Verifique os campos do formulário de Pessoa Física.');
      return;
    }

    const payload: CreatePessoaFisicaRequest = {
      nome: String(this.pfForm.get('nome')?.value ?? '').trim(),
      cpf,
      cep,
      numero: String(this.pfForm.get('numero')?.value ?? '').trim(),
      complemento: String(this.pfForm.get('complemento')?.value ?? '').trim() || undefined,
    };

    this.loadingPf = true;

    // Após cadastrar, recarrega lista imediatamente (evita item vazio / só aparecer no refresh)
    this.pfApi
      .create(payload)
      .pipe(
        switchMap(() => this.pfApi.getAll()),
        finalize(() => (this.loadingPf = false))
      )
      .subscribe({
        next: (list) => {
          this.pessoasFisicas = list ?? [];
          this.showNotice('success', 'Pessoa Física cadastrada com sucesso.');
          this.pfForm.reset();
          this.pfCepInfo = null;
        },
        error: (err) => {
          this.showNotice('error', this.getHttpError(err) || 'Falha ao cadastrar Pessoa Física.');
        },
      });
  }

  criarPJ(): void {
    this.pjForm.markAllAsTouched();

    const cnpj = this.onlyDigits(this.pjForm.get('cnpj')?.value, 14);
    const cep = this.onlyDigits(this.pjForm.get('cep')?.value, 8);

    this.pjForm.patchValue({ cnpj, cep }, { emitEvent: false });

    if (this.pjForm.invalid) {
      this.showNotice('error', 'Verifique os campos do formulário de Pessoa Jurídica.');
      return;
    }

    const payload: CreatePessoaJuridicaRequest = {
      razaoSocial: String(this.pjForm.get('razaoSocial')?.value ?? '').trim(),
      cnpj,
      cep,
      numero: String(this.pjForm.get('numero')?.value ?? '').trim(),
      complemento: String(this.pjForm.get('complemento')?.value ?? '').trim() || undefined,
    };

    this.loadingPj = true;

    this.pjApi
      .create(payload)
      .pipe(
        switchMap(() => this.pjApi.getAll()),
        finalize(() => (this.loadingPj = false))
      )
      .subscribe({
        next: (list) => {
          this.pessoasJuridicas = list ?? [];
          this.showNotice('success', 'Pessoa Jurídica cadastrada com sucesso.');
          this.pjForm.reset();
          this.pjCepInfo = null;
        },
        error: (err) => {
          this.showNotice('error', this.getHttpError(err) || 'Falha ao cadastrar Pessoa Jurídica.');
        },
      });
  }

  // ---------------------------
  // Notice helpers
  // ---------------------------
  clearNotice(): void {
    this.notice = null;
    if (this.noticeTimer) {
      clearTimeout(this.noticeTimer);
      this.noticeTimer = null;
    }
  }

  private showNotice(type: NoticeType, text: string, autoCloseMs = 7000): void {
    this.notice = { type, text };

    if (this.noticeTimer) clearTimeout(this.noticeTimer);
    this.noticeTimer = setTimeout(() => {
      this.notice = null;
      this.noticeTimer = null;
    }, autoCloseMs);
  }

  private getHttpError(err: any): string {
    // Angular HttpErrorResponse costuma vir com: err.error / err.message
    if (!err) return '';
    if (typeof err === 'string') return err;

    const e = err.error;
    if (e) {
      if (typeof e === 'string') return e;
      if (typeof e.message === 'string') return e.message;
      if (typeof e.title === 'string') return e.title;
    }

    if (typeof err.message === 'string') return err.message;
    return '';
  }

  // ---------------------------
  // Sanitização de dígitos
  // ---------------------------
  private bindDigitsOnly(form: FormGroup, controlName: string, maxLen: number | null): void {
    const ctrl = form.get(controlName);
    if (!ctrl) return;

    ctrl.valueChanges.subscribe((v) => {
      const digits = this.onlyDigits(v, maxLen);
      if (v !== digits) {
        ctrl.setValue(digits, { emitEvent: false });
      }
    });
  }

  private onlyDigits(value: any, maxLen: number | null): string {
    let s = String(value ?? '');
    s = s.replace(/\D/g, '');
    if (maxLen && s.length > maxLen) s = s.substring(0, maxLen);
    return s;
  }
}
