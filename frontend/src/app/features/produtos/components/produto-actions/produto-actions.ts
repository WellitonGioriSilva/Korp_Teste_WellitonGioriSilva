import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Produto } from '../../models/produto.model';

@Component({
  selector: 'app-produto-actions',
  imports: [
    RouterLink,
  ],
  templateUrl: './produto-actions.html',
})
export class ProdutoActions {

  @Input({ required: true }) produto!: Produto;

  @Output() delete = new EventEmitter<Produto>();
}
