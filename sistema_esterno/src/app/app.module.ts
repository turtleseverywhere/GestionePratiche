import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { LoggerModule, NgxLoggerLevel } from "ngx-logger";
import { HttpClient, HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { ConfigService } from "@services/config.service";
import { StateService } from "@services/state.service";
import { Interceptor } from '@services/interceptor.service';
import { AuthService } from '@services/auth.service';
import { AuthGuard } from '@services/auth.guard';
import { SignalRService } from '@services/signalR.service';

import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { NZ_I18N } from 'ng-zorro-antd/i18n';
import { en_US } from 'ng-zorro-antd/i18n';
import { registerLocaleData } from '@angular/common';
import en from '@angular/common/locales/en';
import { FormsModule } from '@angular/forms';

import { NgZorroImportsModule } from './ngZorroImports.module';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { IconDefinition } from '@ant-design/icons-angular';
//questo carica solo le icone indicate
//import { AccountBookFill, AlertFill, AlertOutline, SettingOutline } from '@ant-design/icons-angular/icons';
//const icons: IconDefinition[] = [AccountBookFill, AlertOutline, AlertFill, SettingOutline];
//questo carica tutte le icone
import * as AllIcons from '@ant-design/icons-angular/icons';
import config from '../assets/config.json';
const antDesignIcons = AllIcons as {
  [key: string]: IconDefinition;
};
const icons: IconDefinition[] = Object.keys(antDesignIcons).map(key => antDesignIcons[key]);

registerLocaleData(en);

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    RouterModule,
    BrowserModule,
    CommonModule,
    HttpClientModule,
    LoggerModule.forRoot({
      serverLoggingUrl: config.backendUrl + 'Log/Log',
      level: NgxLoggerLevel.DEBUG,
      serverLogLevel: NgxLoggerLevel.ERROR
    }),

    AppRoutingModule,
    BrowserAnimationsModule,
    FormsModule,
    NgZorroImportsModule,
    NzIconModule.forRoot(icons),
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      }
    })
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: Interceptor,
      multi: true,
    },
    ConfigService,
    StateService,
    AuthService,
    AuthGuard,
    SignalRService,
    { provide: NZ_I18N, useValue: en_US }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http);
}
