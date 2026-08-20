import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';

import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast-container',
  imports: [
    AsyncPipe,
  ],
  templateUrl: './toast-container.html',
})
export class ToastContainer {

  readonly toastService = inject(ToastService);

  readonly toasts$ = this.toastService.toasts$;
}
