import { AsyncPipe, DatePipe, DecimalPipe, NgClass } from '@angular/common';
import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  BehaviorSubject,
  EMPTY,
  filter,
  finalize,
  map,
  Subscription,
  switchMap,
  take,
  tap,
} from 'rxjs';

import { NotaFiscal, StatusNotaFiscal } from '../../models/nota-fiscal.model';
import { NotaFiscalService } from '../../services/nota-fiscal.service';
import { ImpressaoNotaSseService } from '../../services/impressao-nota-sse.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-nota-fiscal-view',
  imports: [
    AsyncPipe,
    DatePipe,
    DecimalPipe,
    NgClass,
    RouterLink,
  ],
  templateUrl: './nota-fiscal-view.html',
})
export class NotaFiscalView implements OnInit, OnDestroy {

  private readonly route = inject(ActivatedRoute);

  private readonly notaFiscalService = inject(NotaFiscalService);
  private readonly impressaoSseService = inject(ImpressaoNotaSseService);

  private readonly toastService = inject(ToastService);

  private readonly notaFiscalSubject = new BehaviorSubject<NotaFiscal | null>(null);

  private readonly carregandoSubject = new BehaviorSubject<boolean>(false);
  private readonly imprimindoSubject = new BehaviorSubject<boolean>(false);
  private readonly imprimindoNotaNumeroSubject = new BehaviorSubject<number | null>(null);
  private impressaoSubscription: Subscription | null = null;

  readonly notaFiscal$ = this.notaFiscalSubject.asObservable();

  readonly carregando$ = this.carregandoSubject.asObservable();
  readonly imprimindo$ = this.imprimindoSubject.asObservable();
  readonly imprimindoNotaNumero$ = this.imprimindoNotaNumeroSubject.asObservable();

  ngOnInit(): void {
    this.carregarNotaFiscal();
  }

  ngOnDestroy(): void {
    this.impressaoSubscription?.unsubscribe();
  }

  carregarNotaFiscal(): void {
    this.route.paramMap
      .pipe(
        take(1),
        map((params) => Number(params.get('id')) || null),
        switchMap((id) => {
          if (id === null) {
            return EMPTY;
          }

          this.carregandoSubject.next(true);

          return this.notaFiscalService.findOneById(id).pipe(
            tap((response) => {
              this.notaFiscalSubject.next(response.data);
            }),
            finalize(() => {
              this.carregandoSubject.next(false);
            })
          );
        })
      )
      .subscribe({
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

    if (this.imprimindoSubject.value) return;

    this.imprimindoSubject.next(true);
    this.imprimindoNotaNumeroSubject.next(notaFiscal.numero);

    this.impressaoSubscription = this.impressaoSseService.conectar(notaFiscal.id)
      .pipe(
        filter((event) => event.status === StatusNotaFiscal.Fechada || event.status === StatusNotaFiscal.Erro),
        take(1),
        finalize(() => {
          this.imprimindoSubject.next(false);
          this.imprimindoNotaNumeroSubject.next(null);
          this.impressaoSubscription = null;
        })
      )
      .subscribe({
        next: (event) => {
          this.notaFiscalSubject.next({
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
          this.notaFiscalSubject.next(response.data);
        },

        error: (error) => {
          this.impressaoSubscription?.unsubscribe();
          this.imprimindoSubject.next(false);
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
}
