import { AsyncPipe, DecimalPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { BehaviorSubject, combineLatest, finalize, map, startWith } from 'rxjs';

import { Produto } from '../../../produtos/models/produto.model';
import { ProdutoService } from '../../../produtos/services/produto.service';
import { CreateItemNotaFiscal, CreateNotaFiscal } from '../../models/nota-fiscal.model';
import { NotaFiscalService } from '../../services/nota-fiscal.service';
import { ToastService } from '../../../../shared/services/toast.service';

interface NotaFiscalItemLista extends CreateItemNotaFiscal {
  total: number;
}

type NotaFiscalItemFormGroup = FormGroup<{
  produtoId: FormControl<number | null>;
  quantidade: FormControl<number>;
  valorUnitario: FormControl<number>;
}>;

@Component({
  selector: 'app-nota-fiscal-form',
  imports: [
    AsyncPipe,
    DecimalPipe,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './nota-fiscal-form.html',
  styleUrl: './nota-fiscal-form.css',
})
export class NotaFiscalForm implements OnInit {

  private readonly router = inject(Router);

  private readonly notaFiscalService = inject(NotaFiscalService);

  private readonly produtoService = inject(ProdutoService);

  private readonly toastService = inject(ToastService);

  private readonly produtosSubject = new BehaviorSubject<Produto[]>([]);

  private readonly itensSubject = new BehaviorSubject<NotaFiscalItemLista[]>([]);

  private readonly carregandoSubject = new BehaviorSubject<boolean>(false);

  private readonly salvandoSubject = new BehaviorSubject<boolean>(false);

  readonly produtos$ = this.produtosSubject.asObservable();

  readonly itens$ = this.itensSubject.asObservable();

  readonly carregando$ = this.carregandoSubject.asObservable();

  readonly salvando$ = this.salvandoSubject.asObservable();

  readonly itemForm: NotaFiscalItemFormGroup = new FormGroup({
    produtoId: new FormControl<number | null>(null, {
      validators: [
        Validators.required,
        Validators.min(1),
      ],
    }),
    quantidade: new FormControl(1, {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.min(1),
      ],
    }),
    valorUnitario: new FormControl(0, {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.min(0),
      ],
    }),
  });

  readonly totalPreview$ = combineLatest([
    this.itemForm.controls.quantidade.valueChanges.pipe(
      startWith(this.itemForm.controls.quantidade.value)
    ),
    this.itemForm.controls.valorUnitario.valueChanges.pipe(
      startWith(this.itemForm.controls.valorUnitario.value)
    ),
  ]).pipe(
    map(([quantidade, valorUnitario]) => quantidade * valorUnitario)
  );

  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.carregandoSubject.next(true);

    this.produtoService
      .findAll()
      .pipe(
        finalize(() => {
          this.carregandoSubject.next(false);
        })
      )
      .subscribe({
        next: (response) => {
          this.produtosSubject.next(response.data);
        },

        error: (error) => {
          this.toastService.errorFromResponse(error);
        },
      });
  }

  adicionarItem(): void {
    this.itemForm.markAllAsTouched();

    if (this.itemForm.invalid) {
      return;
    }

    const produtoId = this.itemForm.controls.produtoId.value ?? 0;
    const produto = this.produtosSubject
      .value
      .find((produtoItem) => produtoItem.id === produtoId);

    if (!produto) {
      return;
    }

    const quantidade = this.itemForm.controls.quantidade.value;
    const valorUnitario = this.itemForm.controls.valorUnitario.value;
    const itensAtuais = this.itensSubject.value;
    const itemExistente = itensAtuais.find((item) => item.produtoId === produtoId);
    const quantidadeAtual = itemExistente?.quantidade ?? 0;
    const quantidadeTotal = quantidadeAtual + quantidade;

    if (quantidadeTotal > produto.saldo) {
      this.toastService.error(`Saldo insuficiente para o produto ${produto.descricao}.`);
      return;
    }

    if (itemExistente) {
      this.itensSubject.next(
        itensAtuais.map((item) => {
          if (item.produtoId !== produtoId) {
            return item;
          }

          return {
            ...item,
            quantidade: quantidadeTotal,
            valorUnitario,
            total: quantidadeTotal * valorUnitario,
          };
        })
      );
    } else {
      this.itensSubject.next([
        ...itensAtuais,
        {
          produtoId,
          descricaoProduto: produto.descricao,
          quantidade,
          valorUnitario,
          total: quantidade * valorUnitario,
        },
      ]);
    }

    this.itemForm.reset({
      produtoId: null,
      quantidade: 1,
      valorUnitario: 0,
    });
  }

  removerItem(produtoId: number): void {
    this.itensSubject.next(
      this.itensSubject.value.filter((item) => item.produtoId !== produtoId)
    );
  }

  salvar(): void {
    const itens = this.itensSubject.value;

    if (!itens.length) {
      this.toastService.error('Adicione pelo menos um item na nota fiscal.');
      return;
    }

    const notaFiscal: CreateNotaFiscal = {
      itens: itens.map(({ total, ...item }) => item),
    };

    this.salvandoSubject.next(true);

    this.notaFiscalService
      .create(notaFiscal)
      .pipe(
        finalize(() => {
          this.salvandoSubject.next(false);
        })
      )
      .subscribe({
        next: (response) => {
          this.toastService.success(response.message);
          this.router.navigate(['/notas-fiscais', response.data.id]);
        },

        error: (error) => {
          this.toastService.errorFromResponse(error);
        },
      });
  }

  calcularTotalNota(): number {
    return this.itensSubject.value.reduce(
      (total, item) => total + item.total,
      0
    );
  }
}
