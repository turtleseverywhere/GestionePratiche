import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthGuard } from '@services/auth.guard';
import { NgZorroImportsModule } from '../../ngZorroImports.module';
import { HomeComponent } from './pages/home/home.component';

const routes: Routes = [
  {
    path: 'home',
    component: HomeComponent,
    canActivate: [AuthGuard],
    data: {
      claim: ''
    }
  }
];

@NgModule({
  declarations: [
    HomeComponent,
    //RolesComponent,
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

export class HomeModule {
  constructor() {
    //console.log('ProjectModule loaded.');
  }
}
