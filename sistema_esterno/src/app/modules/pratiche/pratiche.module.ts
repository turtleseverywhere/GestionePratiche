import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AuthGuard } from '@services/auth.guard';
import { NgZorroImportsModule } from '../../ngZorroImports.module';
import { PraticheComponent } from './pages/pratiche/pratiche.component';

const routes: Routes = [
  {
    path: 'pratiche',
    component: PraticheComponent,
    canActivate: [AuthGuard],
    data: {
      claim: 'CanManagePratiche'
    }
  }
];

@NgModule({
  declarations: [
    PraticheComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgZorroImportsModule,
    TranslateModule,
    RouterModule.forChild(routes)
  ],
  providers: [],
  exports: []
})

export class PraticheModule {
  constructor() {
    //console.log('ProjectModule loaded.');
  }
}
