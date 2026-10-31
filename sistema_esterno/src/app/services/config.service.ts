import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { StateService } from '@services/state.service';
export interface Configs {
  backendUrl: string;
  loginFragment: string;
  refreshTokensFragment: string;
}

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private config: Configs;

  constructor(private http: HttpClient,
    private stateService: StateService) { }

  loadConfigs() {
    this.http
      .get('/assets/config.json')
      .subscribe((data => {
        this.config =
        {
          backendUrl: data['backendUrl'],
          loginFragment: data['loginFragment'],
          refreshTokensFragment: data['refreshTokensFragment']
        }
        this.stateService.setConfig(data);
      }));
  }

  getConfig() {
    return this.config;
  }

  getBackendUrl() {
    return this.config.backendUrl;
  }

  getLoginFragment() {
    return this.config.loginFragment;
  }

  getRefreshTokensFragment() {
    return this.config.refreshTokensFragment;
  }
}
