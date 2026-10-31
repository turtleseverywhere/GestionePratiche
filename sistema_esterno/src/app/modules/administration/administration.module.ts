import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AuthGuard } from '@services/auth.guard';
import { NgZorroImportsModule } from '../../ngZorroImports.module';
import { UsersComponent } from './pages/users/users.component';
import { RolesComponent } from './pages/roles/roles.component';

const routes: Routes = [
  {
    path: '',
    redirectTo: '/home',
    pathMatch: 'full'
  },
  {
    path: 'users',
    component: UsersComponent,
    canActivate: [AuthGuard],
    data: {
      claim: 'CANDOANYTHING'
    }
  },
  {
    path: 'roles',
    component: RolesComponent,
    canActivate: [AuthGuard],
    data: {
      claim: 'CANDOANYTHING'
    }
  }
];

@NgModule({
  declarations: [
    UsersComponent,
    RolesComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgZorroImportsModule,
    TranslateModule,
    RouterModule.forChild(routes),
  ],
  providers: [],
  exports: []
})

export class AdministrationModule {
}
