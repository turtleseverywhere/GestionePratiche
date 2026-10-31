import { Component } from '@angular/core';
import { Subject } from 'rxjs';
import { ApiService } from '@services/api.service';
import { takeUntil } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { ConfigService } from '@services/config.service';
import { StateService } from '@services/state.service';
import { AuthService } from '@services/auth.service';
import { SignalRService } from '@services/signalR.service';
import { NGXLogger } from "ngx-logger";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent {
  user: any;
  language: any;
  sideNavIsVisible: boolean = false;
  notificationsDrawerIsVisible: boolean = false;
  pageIsLoading: boolean = false;
  public hasNotifications: boolean = false;
  public notifications: any = [];
  private subscriptions: Subject<any> = new Subject();

  constructor(public translateService: TranslateService,
    private configService: ConfigService,
    private stateService: StateService,
    private apiService: ApiService,
    private authService: AuthService,
    private signalRService: SignalRService,
    //private router: Router,
    private logger: NGXLogger) {

    this.translateService.addLangs(['en', 'it']);
    this.translateService.setDefaultLang('en');

    this.stateService.getLanguage()
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (language => {
          this.language = language;
          this.translateService.use(this.language);
        }),
        error: (error => {
          this.logger.error(error);
        })
      });

    this.configService.loadConfigs();
  }

  ngOnInit() {
    let gettingNotifications = false;
    this.stateService.getUser()
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (user => {
          //console.log("user", user);
          this.user = user;
          if (user) {
            //console.log("getting notifications user not null", gettingNotifications);
            this.signalRService.startSignalR();
            let initialNotificationsInterval = setInterval(() => {
              //console.log("getting configs");
              let configs = this.configService.getConfig();
              if (configs) {
                let backendUrl = this.configService.getBackendUrl();
                if (backendUrl) {
                  if (!gettingNotifications) {
                    gettingNotifications = true;
                    //console.log("get notifications constructor !");
                    //console.log(this.notifications);
                    this.getNotifications();
                    clearInterval(initialNotificationsInterval);
                  }
                  else
                    clearInterval(initialNotificationsInterval);
                }
              }
            }, 100);
          }
          else {
            gettingNotifications = false;
            this.closeNotificationsDrawer();
            //console.log("getting notification user null", gettingNotifications);
            this.signalRService.stopSignalR();
            //console.log("no user constructor :(");
          }
        }),
        error: (error => {
          this.logger.error(error);
        })
      });

    this.stateService.getHasNotifications()
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (hasNotifications => {
          if (hasNotifications) {
            this.hasNotifications = true;
          }
        }),
        error: (error => {
          this.logger.error(error);
        })
      });
  }

  openSideNav(): void {
    this.sideNavIsVisible = true;
  }

  closeSideNav(): void {
    this.sideNavIsVisible = false;
  }

  openNotificationsDrawer(): void {
    this.notificationsDrawerIsVisible = true;
    this.hasNotifications = false;

    //console.log("get notifications query !");
    this.getNotifications();
  }

  closeNotificationsDrawer(): void {
    //console.log("close drawer !");
    this.notificationsDrawerIsVisible = false;
    this.hasNotifications = false;
    if (this.user)
      this.setUnreadNotificationsAsRead();
  }

  getNotifications() {
    this.pageIsLoading = true;
    this.apiService.getNotifications()
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.notifications = response.notifications;
          for (let i = 0; i < this.notifications.length; i++) {
            if (!this.notifications[i].readDate) {
              this.hasNotifications = true;
              break;
            }
          }
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }

  setUnreadNotificationsAsRead() {
    this.pageIsLoading = true;
    this.apiService.setUnreadNotificationsAsRead()
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }

  deleteNotification(notificationId: string) {
    this.pageIsLoading = true;
    this.apiService.deleteNotification(notificationId)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.getNotifications();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }

  logout() {
    this.authService.logOut();
  }

  checkClaim(claim) {
    return this.authService.checkClaim(claim);
  }

  changeLanguage(lang) {
    this.translateService.use(lang);
    this.stateService.setLanguage(lang);
  }

  ngOnDestroy(): void {
    this.subscriptions.next({});
    this.subscriptions.complete();
  }
}
