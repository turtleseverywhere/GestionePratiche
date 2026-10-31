import { Component } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NGXLogger } from 'ngx-logger';
import { ApiService } from '@services/api.service';

@Component({
  selector: 'signalR-component',
  templateUrl: './signalR.component.html',
  styleUrls: ['./signalR.component.less']
})
export class SignalRComponent {
  pageIsLoading: boolean = false;
  private subscriptions: Subject<any> = new Subject();

  constructor(
    private apiService: ApiService,
    private logger: NGXLogger) {
  }


  //<send messages>
  selectedConnectionId: string = '';
  selectedClientId: string = '';
  selectedUserId: string = ''; //d93bafb1-a5ff-41ce-93b2-01b3137588f4
  messageToSend: string = '';

  sendMessageToBroadcast() {
    this.pageIsLoading = true;
    this.apiService.sendMessageToBroadcast(this.messageToSend)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (users => {
          this.pageIsLoading = false;
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }
  sendMessageToClient() {
    this.pageIsLoading = true;
    this.apiService.sendMessageToClient(this.selectedClientId, this.messageToSend)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (users => {
          this.pageIsLoading = false;
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }
  sendMessageToConnection() {
    this.pageIsLoading = true;
    this.apiService.sendMessageToConnection(this.selectedConnectionId, this.messageToSend)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (users => {
          this.pageIsLoading = false;
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }
  sendMessageToUser() {
    this.pageIsLoading = true;
    this.apiService.sendMessageToUser(this.selectedUserId, this.messageToSend)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (users => {
          this.pageIsLoading = false;
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }
  //</send messages>

  ngOnDestroy(): void {
    this.subscriptions.next({});
    this.subscriptions.complete();
  }
}
