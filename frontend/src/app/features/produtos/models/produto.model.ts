export interface Produto {
  id: number;
  descricao: string;
  saldo: number;
}

export interface CreateProduto {
  descricao: string;
  saldo: number;
}

export type UpdateProduto = CreateProduto;
