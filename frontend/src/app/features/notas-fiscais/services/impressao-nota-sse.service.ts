import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  ImpressaoNotaEvent
} from '../models/impressao-nota.model';

interface ImpressaoNotaEventResponse {
  NotaFiscalId?: number;
  Status?: ImpressaoNotaEvent['status'];
  Message?: string;
}

@Injectable({
  providedIn: 'root',
})
export class ImpressaoNotaSseService {

  conectar(
    notaFiscalId: number
  ): Observable<ImpressaoNotaEvent> {

    return new Observable<ImpressaoNotaEvent>(
      (subscriber) => {

        const eventSource = new EventSource(
          `${environment.faturamentoApi}/NotaFiscal/${notaFiscalId}/stream`
        );
        
        // Ouvindo eventos do servidor
        eventSource.onmessage = (event) => {

          const data = JSON.parse(event.data) as ImpressaoNotaEventResponse;

          subscriber.next({
            notaFiscalId: data.NotaFiscalId ?? notaFiscalId,
            status: data.Status!,
            message: data.Message,
          });
        };

        // Ouvindo erros de conexão
        eventSource.onerror = (error) => {

          subscriber.error(error);

          eventSource.close();
        };

        // Fechando a conexão quando o Observable for cancelado
        return () => {
          eventSource.close();
        };
      }
    );
  }
}
