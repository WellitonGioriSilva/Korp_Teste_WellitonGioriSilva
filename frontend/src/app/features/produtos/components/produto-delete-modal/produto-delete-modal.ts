import { Component, EventEmitter, Input, Output } from '@angular/core';

import { Produto } from '../../models/produto.model';

@Component({
  selector: 'app-produto-delete-modal',
  templateUrl: './produto-delete-modal.html',
})
export class ProdutoDeleteModal {

  @Input({ required: true }) produto!: Produto;

  @Input() carregando = false;

  @Output() cancel = new EventEmitter<void>();

  @Output() confirm = new EventEmitter<Produto>();
}
