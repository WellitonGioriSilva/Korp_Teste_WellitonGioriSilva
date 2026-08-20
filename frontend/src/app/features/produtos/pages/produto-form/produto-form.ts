import { AsyncPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BehaviorSubject, EMPTY, finalize, map, switchMap, take, tap } from 'rxjs';

import { CreateProduto } from '../../models/produto.model';
import { ProdutoService } from '../../services/produto.service';
import { ToastService } from '../../../../shared/services/toast.service';

type ProdutoFormGroup = FormGroup<{
  descricao: FormControl<string>;
  saldo: FormControl<number>;
}>;

@Component({
  selector: 'app-produto-form',
  imports: [
    AsyncPipe,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './produto-form.html',
  styleUrl: './produto-form.css',
})
export class ProdutoForm implements OnInit {

  private readonly route = inject(ActivatedRoute);

  private readonly router = inject(Router);

  private readonly produtoService = inject(ProdutoService);

  private readonly toastService = inject(ToastService);

  private readonly carregandoSubject = new BehaviorSubject<boolean>(false);

  private readonly salvandoSubject = new BehaviorSubject<boolean>(false);

  readonly carregando$ = this.carregandoSubject.asObservable();

  readonly salvando$ = this.salvandoSubject.asObservable();

  readonly produtoId$ = this.route.paramMap.pipe(
    map((params) => Number(params.get('id')) || null)
  );

  readonly modoEdicao$ = this.produtoId$.pipe(
    map((id) => id !== null)
  );

  readonly form: ProdutoFormGroup = new FormGroup({
    descricao: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(1),
        Validators.maxLength(100),
      ],
    }),
    saldo: new FormControl(0, {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.min(0),
      ],
    }),
  });

  ngOnInit(): void {
    this.produtoId$
      .pipe(
        take(1),
        switchMap((id) => {
          if (id === null) {
            return EMPTY;
          }

          this.carregandoSubject.next(true);
          return this.produtoService.findOneById(id).pipe(
            tap((response) => {
              this.form.patchValue({
                descricao: response.data.descricao,
                saldo: response.data.saldo,
              });
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

  salvar(): void {
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      return;
    }

    const produto: CreateProduto = this.form.getRawValue();

    this.produtoId$
      .pipe(
        take(1),
        tap(() => {
          this.salvandoSubject.next(true);
        }),
        switchMap((id) => {
          if (id === null) {
            return this.produtoService.create(produto);
          }

          return this.produtoService.update(id, produto);
        }),
        finalize(() => {
          this.salvandoSubject.next(false);
        })
      )
      .subscribe({
        next: (response) => {
          this.toastService.success(response.message);
          this.router.navigate(['/produtos']);
        },

        error: (error) => {
          this.toastService.errorFromResponse(error);
        },
      });
  }
}
