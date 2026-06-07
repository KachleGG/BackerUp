import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Layout } from './components/layout/layout';
import { Dashboard } from './pages/dashboard/dashboard';
import { Clients } from './pages/clients/clients';
import { ClientForm } from './pages/clients/client-form';
import { BackupJobs } from './pages/backup-jobs/backup-jobs';
import { BackupJobForm } from './pages/backup-jobs/backup-job-form';
import { Users } from './pages/users/users';
import { UserForm } from './pages/users/user-form';
import { Logs } from './pages/logs/logs';
import { LogForm } from './pages/logs/log-form';

export const routes: Routes = [
  { path: 'login', component: Login },
  {
    path: '',
    component: Layout,
    children: [
      { path: 'dashboard', component: Dashboard },
      { path: 'clients', component: Clients },
      { path: 'clients/add', component: ClientForm },
      { path: 'clients/edit/:id', component: ClientForm },
      { path: 'backup-jobs', component: BackupJobs },
      { path: 'backup-jobs/add', component: BackupJobForm },
      { path: 'backup-jobs/edit/:id', component: BackupJobForm },
      { path: 'users', component: Users },
      { path: 'users/add', component: UserForm },
      { path: 'users/edit/:id', component: UserForm },
      // job-clients removed - management moved into clients and backup-jobs pages
      { path: 'logs', component: Logs },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];

