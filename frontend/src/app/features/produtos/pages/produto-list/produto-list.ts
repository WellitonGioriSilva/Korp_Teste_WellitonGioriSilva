import { AsyncPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { BehaviorSubject, catchError, debounceTime, distinctUntilChanged, finalize, map, of, startWith, switchMap, tap } from 'rxjs';

import { Produto } from '../../models/produto.model';
import { ProdutoService } from '../../services/produto.service';
import { ApiResponse } from '../../../../shared/models/api-response.model';
import { ProdutoActions } from '../../components/produto-actions/produto-actions';
import { ProdutoDeleteModal } from '../../components/produto-delete-modal/produto-delete-modal';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-produto-list',
  imports: [
    AsyncPipe,
    ProdutoActions,
    ProdutoDeleteModal,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './produto-list.html',
})
export class ProdutoList implements OnInit {

  private readonly produtoService = inject(ProdutoService);

  private readonly toastService = inject(ToastService);

  private readonly produtosSubject = new BehaviorSubject<Produto[]>([]);

  private readonly carregandoSubject = new BehaviorSubject<boolean>(false);

  readonly buscaControl = new FormControl<string>('', { nonNullable: true });

  readonly carregando$ = this.carregandoSubject.asObservable();

  readonly produtos$ = this.produtosSubject.asObservable();

  ngOnInit(): void {
    this.buscaControl
      .valueChanges
      .pipe(
        startWith(this.buscaControl.value),
        debounceTime(300),
        map((busca) => busca.trim()),
        distinctUntilChanged(),
        tap(() => {
          this.carregandoSubject.next(true);
        }),
        switchMap((busca) =>
          this.produtoService.findAll(busca).pipe(
            catchError((error) => {
              this.toastService.errorFromResponse(error);

              return of({
                error: true,
                message: '',
                code: 0,
                data: this.produtosSubject.value,
              } satisfies ApiResponse<Produto[]>);
            }),
            finalize(() => {
              this.carregandoSubject.next(false);
            })
          )
        )
      )
      .subscribe({
        next: (response: ApiResponse<Produto[]>) => {
          this.produtosSubject.next(response.data);
        },
      });
  }

  carregarProdutos(): void {
    this.buscaControl.setValue(this.buscaControl.value);
  }
}
