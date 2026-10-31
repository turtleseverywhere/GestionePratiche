import { Component } from '@angular/core';
import { StateService } from '@services/state.service';
import { AuthService } from '@services/auth.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NGXLogger } from 'ngx-logger';

@Component({
  selector: 'pratiche-component',
  templateUrl: './pratiche.component.html',
  styleUrls: ['./pratiche.component.less']
})
export class PraticheComponent {
  user: any;
  selectedUsername: string = '';
  selectedPassword: string = '';
  pageIsLoading: boolean = false;
  private subscriptions: Subject<any> = new Subject();

  constructor(private authService: AuthService,
    private stateService: StateService,
    private logger: NGXLogger) {
    this.stateService.getUser()
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (user => {
          this.user = user;
        }),
        error: (error => {
          this.logger.error(error);
        })
      });
  }
  login() {
    this.pageIsLoading = true;
    this.authService.logIn(this.selectedUsername, this.selectedPassword)
      .then(result => {
        this.pageIsLoading = false;
      }).catch((error) => {
        this.logger.error(error);
        this.pageIsLoading = false;
      });
  }
  ngOnDestroy(): void {
    this.subscriptions.next({});
    this.subscriptions.complete();
  }
}
