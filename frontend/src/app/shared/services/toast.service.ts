import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';

import { ApiResponse } from '../models/api-response.model';

export type ToastType = 'success' | 'error';

export interface ToastMessage {
  id: number;
  message: string;
  type: ToastType;
}

@Injectable({
  providedIn: 'root',
})
export class ToastService {

  private readonly toastsSubject = new BehaviorSubject<ToastMessage[]>([]);

  private nextId = 1;

  readonly toasts$ = this.toastsSubject.asObservable();

  success(message: string): void {
    this.show(message, 'success');
  }

  error(message: string): void {
    this.show(message, 'error');
  }

  errorFromResponse(error: unknown): void {
    this.error(this.getApiMessage(error));
  }

  remove(id: number): void {
    this.toastsSubject.next(
      this.toastsSubject.value.filter((toast) => toast.id !== id)
    );
  }

  private show(message: string, type: ToastType): void {
    const toast: ToastMessage = {
      id: this.nextId++,
      message,
      type,
    };

    this.toastsSubject.next([
      ...this.toastsSubject.value,
      toast,
    ]);

    window.setTimeout(() => {
      this.remove(toast.id);
    }, 5000);
  }

  private getApiMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const apiError = error.error as Partial<ApiResponse<unknown>> | null;

      if (apiError?.message) {
        return apiError.message;
      }
    }

    return 'Erro inesperado.';
  }
}
