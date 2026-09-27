import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // Rotas públicas
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'esqueceu-senha',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/esqueceu-senha/esqueceu-senha.component').then(m => m.EsqueceuSenhaComponent),
  },
  {
    path: 'redefinir-senha',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/redefinir-senha/redefinir-senha.component').then(m => m.RedefinirSenhaComponent),
  },

  // Shell autenticado — todas as rotas internas ficam aqui
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./core/layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
      },
      {
        path: 'usuarios',
        loadComponent: () => import('./features/usuarios/usuarios-list/usuarios-list.component').then(m => m.UsuariosListComponent),
      },
      // Rotas futuras
      { path: 'beneficiarios',  loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent) },
      { path: 'doadores',       loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent) },
      { path: 'visitas',        loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent) },
      { path: 'comunidades',    loadComponent: () => import('./features/comunidades/comunidades-list/comunidades-list.component').then(m => m.ComunidadesListComponent) },
      { path: 'mapa',           loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent) },
      { path: 'setores',        loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent) },
      { path: 'relatorios',     loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent) },
    ],
  },

  { path: '**', redirectTo: '/dashboard' },
];
