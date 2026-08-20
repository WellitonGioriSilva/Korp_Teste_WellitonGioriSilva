import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../shared/models/api-response.model';
import { CreateNotaFiscal, NotaFiscal } from '../models/nota-fiscal.model';

@Injectable({
  providedIn: 'root',
})
export class NotaFiscalService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.faturamentoApi}/NotaFiscal`;

  findAll(): Observable<ApiResponse<NotaFiscal[]>> {
    return this.http.get<ApiResponse<NotaFiscal[]>>(this.apiUrl);
  }

  findOneById(id: number): Observable<ApiResponse<NotaFiscal>> {
    return this.http.get<ApiResponse<NotaFiscal>>(
      `${this.apiUrl}/${id}`
    );
  }

  create(notaFiscal: CreateNotaFiscal): Observable<ApiResponse<NotaFiscal>> {
    return this.http.post<ApiResponse<NotaFiscal>>(
      this.apiUrl,
      notaFiscal
    );
  }

  imprimir(id: number): Observable<ApiResponse<NotaFiscal>> {
    return this.http.post<ApiResponse<NotaFiscal>>(
      `${this.apiUrl}/${id}/Impressao`,
      {}
    );
  }
}
