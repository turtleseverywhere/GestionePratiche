import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthGuard } from '@services/auth.guard';
import { NgZorroImportsModule } from '../../ngZorroImports.module';
import { PraticheComponent } from './pages/pratiche/pratiche.component';

const routes: Routes = [
  {
    path: 'pratiche',
    component: PraticheComponent,
    canActivate: [AuthGuard],
    data: {
      claim: ''
    }
  }
];

@NgModule({
  declarations: [
    PraticheComponent
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

export class PraticheModule {
  constructor() {
    //console.log('ProjectModule loaded.');
  }
}
