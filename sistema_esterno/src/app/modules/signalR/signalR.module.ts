import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthGuard } from '@services/auth.guard';
import { NgZorroImportsModule } from '../../ngZorroImports.module';
import { SignalRComponent } from './pages/signalR/signalR.component';

const routes: Routes = [
  {
    path: '',
    redirectTo: '/home',
    pathMatch: 'full'
  },
  {
    path: 'signalR',
    component: SignalRComponent,
    canActivate: [AuthGuard],
    data: {
      claim: 'CanRegisterToSignalR'
    }
  }
];

@NgModule({
  declarations: [
    SignalRComponent,
  ],
  imports: [
    RouterModule.forChild(routes),
    TranslateModule,
    CommonModule,
    FormsModule,
    NgZorroImportsModule
  ],
  providers: [],
  exports: []
})
export class SignalRModule { }
