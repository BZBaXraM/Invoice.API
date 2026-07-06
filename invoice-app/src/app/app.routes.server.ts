import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  { path: 'login', renderMode: RenderMode.Prerender },
  { path: 'register', renderMode: RenderMode.Prerender },
  { path: 'confirm-email', renderMode: RenderMode.Prerender },
  { path: 'forgot-password', renderMode: RenderMode.Prerender },
  { path: 'reset-password', renderMode: RenderMode.Prerender },
  // Everything behind the authenticated shell reads its session from
  // localStorage (JWT), which the server cannot see — render client-only.
  { path: '**', renderMode: RenderMode.Client },
];
