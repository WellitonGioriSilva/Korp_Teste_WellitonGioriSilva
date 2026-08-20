import { AsyncPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BehaviorSubject, EMPTY, finalize, map, switchMap, take, tap } from 'rxjs';

import { Produto } from '../../models/produto.model';
import { ProdutoService } from '../../services/produto.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-produto-view',
  imports: [
    AsyncPipe,
    RouterLink,
  ],
  templateUrl: './produto-view.html',
})
export class ProdutoView implements OnInit {

  private readonly route = inject(ActivatedRoute);

  private readonly produtoService = inject(ProdutoService);

  private readonly toastService = inject(ToastService);

  private readonly produtoSubject = new BehaviorSubject<Produto | null>(null);

  private readonly carregandoSubject = new BehaviorSubject<boolean>(false);

  readonly produto$ = this.produtoSubject.asObservable();

  readonly carregando$ = this.carregandoSubject.asObservable();

  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        take(1),
        map((params) => Number(params.get('id')) || null),
        switchMap((id) => {
          if (id === null) {
            return EMPTY;
          }

          this.carregandoSubject.next(true);

          return this.produtoService.findOneById(id).pipe(
            tap((response) => {
              this.produtoSubject.next(response.data);
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
}
