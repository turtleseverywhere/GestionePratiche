import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { StateService } from '@services/state.service';
import { ConfigService } from '@services/config.service';
import { NGXLogger } from 'ngx-logger';
import { AuthService } from '@services/auth.service';
import { Subject, takeUntil } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private initialSignalRConnectionInterval: any;
  private signalRConnection: any;
  private subscriptions: Subject<any> = new Subject();

  constructor(private authService: AuthService,
    private configService: ConfigService,
    private stateService: StateService,
    private logger: NGXLogger) {
  }

  startSignalR() {
    if (!this.signalRConnection) {
      let buildingSignalRConnection = false;

      let initialConfigsInterval = setInterval(() => {
        //console.log("getting configs");
        let configs = this.configService.getConfig();
        if (configs) {
          let backendUrl = this.configService.getBackendUrl();
          let jwtToken = this.stateService.getJWTToken();
          if (backendUrl && jwtToken) {
            if (!buildingSignalRConnection) {
              buildingSignalRConnection = true;
              this.buildSignalRConnection();
              clearInterval(initialConfigsInterval);
            }
            else
              clearInterval(initialConfigsInterval);
          }
        }
      }, 100);
    }
    else
      this.connectSignalR();
  }

  private buildSignalRConnection() {
    //console.log("building signalr connection..");
    if (!this.signalRConnection) {
      this.signalRConnection = new signalR.HubConnectionBuilder()
        .withUrl(this.configService.getBackendUrl() + "signalRHub"
          , { accessTokenFactory: () => this.stateService.getJWTToken() }
        )
        .configureLogging(signalR.LogLevel.Information)
        //.withAutomaticReconnect([0, 2000, 10000, 10000, 10000, 10000, 10000, 10000, 30000, 30000, 30000])
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: context => {
            const retryTimes = [0, 2000, 10000, 10000, 10000, 10000, 10000, 10000, 30000, 30000, 30000];
            const index = context.previousRetryCount < retryTimes.length ? context.previousRetryCount : retryTimes.length - 1;
            this.handleSignalRError(context.retryReason);
            //console.log(context);
            return retryTimes[index];
          }
        })
        .build();

      this.signalRConnection.onclose(async (error) => {
        console.log("onclose", error);
        this.handleSignalRError(error);
      });

      this.signalRConnection.onreconnecting((error) => {
        //console.assert(this.signalRConnection.state === signalR.HubConnectionState.Reconnecting);
        console.log("onreconnecting", error);
        this.handleSignalRError(error);
      });

      this.signalRConnection.onreconnected(connectionId => {
        console.log("new Connection ID:", connectionId);
        this.signalRConnection.invoke("RegisterClientConnection", this.stateService.getCachedSignalRConnectionId())
          .then(result => {
            //console.log(result);
            this.stateService.setCachedSignalRConnectionId(this.signalRConnection.connectionId);
          });
      });

      this.signalRConnection.on("ReceiveMessage", async (data) => {
        console.log("Received message from SignalR:", data);
        this.stateService.setHasNotifications(true);
      });
    }
    if (this.signalRConnection)
      this.connectSignalR();
  }

  private connectSignalR() {
    //console.log("connect signalr !");
    //console.log(this.initialSignalRConnectionInterval);
    if (this.signalRConnection && this.signalRConnection.state === signalR.HubConnectionState.Disconnected) {
      //console.log("connection already exists !");
      //console.log(this.initialSignalRConnectionInterval);
      if (this.initialSignalRConnectionInterval) {
        //console.log("clearing interval");
        clearInterval(this.initialSignalRConnectionInterval);
        this.initialSignalRConnectionInterval = null;
      }
      if (!this.initialSignalRConnectionInterval) {
        //console.log("creating new interval");

        let signalRConnectionFunction = async () => {
          console.log("signalr function");
          //console.log("connection", this.signalRConnection);
          //if (this.signalRConnection)
          //  console.log("connection state", this.signalRConnection.state);
          if (this.signalRConnection && this.signalRConnection.state === signalR.HubConnectionState.Disconnected) {
            //console.log("trying initial connection..");
            await this.signalRConnection.start().then(() => {
              //console.log("SignalR connected");
              if (this.stateService.getCachedSignalRConnectionId() == '')
                this.stateService.setCachedSignalRConnectionId(this.signalRConnection.connectionId);

              this.signalRConnection.invoke("RegisterClientConnection", this.stateService.getCachedSignalRConnectionId())
                .then(result => {
                  //console.log(result);
                  this.stateService.setCachedSignalRConnectionId(this.signalRConnection.connectionId);
                });
            }).catch(error => {
              console.log("start", error);
              this.handleSignalRError(error);
            });
          }
          else {
            //console.log("clear interval !");
            clearInterval(this.initialSignalRConnectionInterval);
            this.initialSignalRConnectionInterval = null;
          }
        };
        signalRConnectionFunction();
        this.initialSignalRConnectionInterval = setInterval(signalRConnectionFunction, 5000);
      }
    }
  }

  handleSignalRError(error: Error) {
    if (error) {
      console.log(error);
      if (error.message.toLowerCase().includes("unauthorized")
        || error.message.includes("401")) {
        //console.log("refreshing !");
        this.authService.tryRefreshToken()
          .pipe(takeUntil(this.subscriptions))
          .subscribe({
            next: (result => {
              if (result != undefined) {
                if (result.token != undefined)
                  this.stateService.setJWTToken(result.token);
                if (result.refreshToken != undefined)
                  this.stateService.setJWTRefreshToken(result.refreshToken);
                if (result.claims != undefined)
                  this.stateService.setClaims(result.claims);
                //console.log("refreshed !");
              }
            }),
            error: (error => {
              //console.log("stop and logout signalr !");
              //if (!this.getIsSignalRDisconnected()) {
              //  console.log("connection", this.signalRConnection);
              //  console.log("connection state", this.signalRConnection.state);
              this.stopSignalR();
              this.authService.logOut();
              //}
            })
          });
      }
    }
  }

  stopSignalR() {
    if (this.signalRConnection) {
      //console.log("stopping connection !");
      this.signalRConnection.stop();
    }
    //if (this.initialSignalRConnectionInterval) {
    //console.log("clearing interval !");
    clearInterval(this.initialSignalRConnectionInterval);
    this.initialSignalRConnectionInterval = null;
    //}
  }

  //getIsSignalRDisconnected() {
  //  let isDisconnected = false;
  //  if (!this.signalRConnection ||
  //    (this.signalRConnection && this.signalRConnection.state === signalR.HubConnectionState.Disconnected))
  //    isDisconnected = true;
  //  return isDisconnected;
  //}

  ngOnDestroy(): void {
    //console.log("destroy signalr service");
    this.stopSignalR();
    this.subscriptions.next({});
    this.subscriptions.complete();
  }
}
