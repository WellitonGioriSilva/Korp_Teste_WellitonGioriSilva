import { AsyncPipe, DatePipe, DecimalPipe, NgClass } from '@angular/common';
import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  BehaviorSubject,
  combineLatest,
  filter,
  finalize,
  map,
  startWith,
  Subscription,
  take,
} from 'rxjs';

import { NotaFiscal, StatusNotaFiscal } from '../../models/nota-fiscal.model';
import { NotaFiscalService } from '../../services/nota-fiscal.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { ImpressaoNotaSseService } from '../../services/impressao-nota-sse.service';

@Component({
  selector: 'app-nota-fiscal-list',
  imports: [
    AsyncPipe,
    DatePipe,
    DecimalPipe,
    NgClass,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './nota-fiscal-list.html',
  styleUrl: './nota-fiscal-list.css',
})
export class NotaFiscalList implements OnInit, OnDestroy {

  private readonly notaFiscalService = inject(NotaFiscalService);
  private readonly impressaoSseService = inject(ImpressaoNotaSseService);

  private readonly toastService = inject(ToastService);

  private readonly notasFiscaisSubject = new BehaviorSubject<NotaFiscal[]>([]);

  private readonly carregandoSubject = new BehaviorSubject<boolean>(false);
  private readonly imprimindoNotaIdSubject = new BehaviorSubject<number | null>(null);
  private readonly imprimindoNotaNumeroSubject = new BehaviorSubject<number | null>(null);
  private impressaoSubscription: Subscription | null = null;

  readonly buscaControl = new FormControl<string>('', { nonNullable: true });

  readonly carregando$ = this.carregandoSubject.asObservable();
  readonly imprimindoNotaId$ = this.imprimindoNotaIdSubject.asObservable();
  readonly imprimindoNotaNumero$ = this.imprimindoNotaNumeroSubject.asObservable();

  readonly notasFiscais$ = combineLatest([
    this.notasFiscaisSubject.asObservable(),
    this.buscaControl.valueChanges.pipe(startWith(this.buscaControl.value)),
  ]).pipe(
    map(([notasFiscais, busca]) => {
      const termo = busca.trim().toLowerCase();

      if (!termo) {
        return notasFiscais;
      }

      return notasFiscais.filter((notaFiscal) =>
        notaFiscal.numero.toString().includes(termo) ||
        notaFiscal.status.toLowerCase().includes(termo)
      );
    })
  );

  ngOnInit(): void {
    this.carregarNotasFiscais();
  }

  ngOnDestroy(): void {
    this.impressaoSubscription?.unsubscribe();
  }

  carregarNotasFiscais(): void {
    this.carregandoSubject.next(true);

    this.notaFiscalService
      .findAll()
      .pipe(
        finalize(() => {
          this.carregandoSubject.next(false);
        })
      )
      .subscribe({
        next: (response) => {
          this.notasFiscaisSubject.next(response.data);
        },

        error: (error) => {
          this.toastService.errorFromResponse(error);
        },
      });
  }

  imprimirNotaFiscal(notaFiscal: NotaFiscal): void {
    if (notaFiscal.status !== StatusNotaFiscal.Aberta) {
      this.toastService.error('Apenas notas fiscais com status "Aberta" podem ser impressas.');
      return;
    }

    if (this.imprimindoNotaIdSubject.value !== null) return;

    this.imprimindoNotaIdSubject.next(notaFiscal.id);
    this.imprimindoNotaNumeroSubject.next(notaFiscal.numero);

    this.impressaoSubscription = this.impressaoSseService.conectar(notaFiscal.id)
      .pipe(
        filter((event) => event.status === StatusNotaFiscal.Fechada || event.status === StatusNotaFiscal.Erro),
        take(1),
        finalize(() => {
          this.imprimindoNotaIdSubject.next(null);
          this.imprimindoNotaNumeroSubject.next(null);
          this.impressaoSubscription = null;
        })
      )
      .subscribe({
        next: (event) => {
          this.atualizarNotaFiscal({
            ...notaFiscal,
            status: event.status,
            observacao: event.message ?? notaFiscal.observacao,
          });

          if (event.status === StatusNotaFiscal.Fechada) {
            this.toastService.success(
              'Nota fiscal impressa e estoque atualizado com sucesso.'
            );

            return;
          }

          this.toastService.error(
            event.message ??
            'Nao foi possivel processar a nota fiscal.'
          );
        },
        error: () => {
          this.toastService.error(
            'A conexao com o processamento da nota foi interrompida.'
          );
        },
      });

    this.notaFiscalService
      .imprimir(notaFiscal.id)
      .subscribe({
        next: (response) => {
          this.atualizarNotaFiscal(response.data);
        },

        error: (error) => {
          this.impressaoSubscription?.unsubscribe();
          this.imprimindoNotaIdSubject.next(null);
          this.imprimindoNotaNumeroSubject.next(null);
          this.toastService.errorFromResponse(error);
        },
      });
  }

  calcularTotal(notaFiscal: NotaFiscal): number {
    return notaFiscal.itens.reduce((total, item) => total + item.valorTotal, 0);
  }

  getStatusClasses(status: StatusNotaFiscal): string {
    if (status === StatusNotaFiscal.Erro) {
      return 'bg-red-100 text-red-800';
    }

    if (status === StatusNotaFiscal.Aberta) {
      return 'bg-yellow-100 text-yellow-800';
    }

    return 'bg-blue-100 text-blue-800';
  }

  private atualizarNotaFiscal(notaFiscalAtualizada: NotaFiscal): void {
    this.notasFiscaisSubject.next(
      this.notasFiscaisSubject.value.map((notaFiscal) =>
        notaFiscal.id === notaFiscalAtualizada.id
          ? notaFiscalAtualizada
          : notaFiscal
      )
    );
  }
}
