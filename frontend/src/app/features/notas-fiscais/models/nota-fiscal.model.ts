export enum StatusNotaFiscal {
  Aberta = 'Aberta',
  Fechada = 'Fechada',
  Processando = 'Processando',
  Erro = 'Erro',
}

export interface ItemNotaFiscal {
  id: number;
  notaFiscalId: number;
  produtoId: number;
  descricaoProduto: string;
  quantidade: number;
  valorUnitario: number;
  valorTotal: number;
}

export interface NotaFiscal {
  id: number;
  numero: number;
  status: StatusNotaFiscal;
  observacao: string | null;
  dataEmissao: string;
  itens: ItemNotaFiscal[];
}

export interface CreateItemNotaFiscal {
  produtoId: number;
  quantidade: number;
  valorUnitario: number;
  descricaoProduto: string;
}

export interface CreateNotaFiscal {
  itens: CreateItemNotaFiscal[];
}
