import { Routes } from '@angular/router';

import { MainLayout } from './layout/main-layout/main-layout';
import { ProdutoList } from './features/produtos/pages/produto-list/produto-list';
import { ProdutoForm } from './features/produtos/pages/produto-form/produto-form';
import { ProdutoView } from './features/produtos/pages/produto-view/produto-view';
import { NotaFiscalList } from './features/notas-fiscais/pages/nota-fiscal-list/nota-fiscal-list';
import { NotaFiscalForm } from './features/notas-fiscais/pages/nota-fiscal-form/nota-fiscal-form';
import { NotaFiscalView } from './features/notas-fiscais/pages/nota-fiscal-view/nota-fiscal-view';

export const routes: Routes = [
  {
    path: '',
    component: MainLayout,
    children: [
      {
        path: '',
        redirectTo: 'produtos',
        pathMatch: 'full',
      },
      {
        path: 'produtos',
        component: ProdutoList,
      },
      {
        path: 'produtos/novo',
        component: ProdutoForm,
      },
      {
        path: 'produtos/:id',
        component: ProdutoView,
      },
      {
        path: 'produtos/:id/update',
        component: ProdutoForm,
      },
      {
        path: 'produtos/:id/editar',
        component: ProdutoForm,
      },
      {
        path: 'notas-fiscais',
        component: NotaFiscalList,
      },
      {
        path: 'notas-fiscais/nova',
        component: NotaFiscalForm,
      },
      {
        path: 'notas-fiscais/:id',
        component: NotaFiscalView,
      },
    ],
  },
];
