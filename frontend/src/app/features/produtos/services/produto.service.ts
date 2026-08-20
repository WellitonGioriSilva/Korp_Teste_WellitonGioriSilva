import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { CreateProduto, Produto, UpdateProduto } from '../models/produto.model';
import { ApiResponse } from '../../../shared/models/api-response.model';

@Injectable({
  providedIn: 'root',
})
export class ProdutoService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.estoqueApi}/Produto`;

  findAll(descricao?: string): Observable<ApiResponse<Produto[]>> {
    let params = new HttpParams();

    if (descricao?.trim()) {
      params = params.set('descricao', descricao.trim());
    }

    return this.http.get<ApiResponse<Produto[]>>(this.apiUrl, {
      params,
    });
  }

  findOneById(id: number): Observable<ApiResponse<Produto>> {
    return this.http.get<ApiResponse<Produto>>(
      `${this.apiUrl}/${id}`
    );
  }

  create(produto: CreateProduto): Observable<ApiResponse<Produto>> {
    return this.http.post<ApiResponse<Produto>>(
      this.apiUrl,
      produto
    );
  }

  update(id: number, produto: UpdateProduto): Observable<ApiResponse<Produto>> {
    return this.http.put<ApiResponse<Produto>>(
      `${this.apiUrl}/${id}`,
      produto
    );
  }
}
