import { Component } from '@angular/core';

@Component({
  selector: 'app-placeholder',
  standalone: true,
  template: `
    <div class="flex items-center justify-center h-64">
      <div class="text-center text-slate-400">
        <i class="pi pi-hammer text-4xl mb-3 block"></i>
        <p class="text-lg font-medium">Em construção</p>
        <p class="text-sm">Este módulo será implementado nos próximos passos.</p>
      </div>
    </div>
  `,
})
export class PlaceholderComponent {}
