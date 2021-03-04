import { Component, OnInit } from '@angular/core';
import * as signalR from "@aspnet/signalr";
import { AppConsts } from '../shared/AppConsts';
import { __core_private_testing_placeholder__ } from '@angular/core/testing';
import Swal from 'sweetalert2'

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  ngOnInit(): void {
    this.startConnection();
  }

  constructor() {

  }

  toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 3000
  })


  connection: signalR.HubConnection
  // stores the ID from SetInterval
  heartBeatTockTimer: any;
  // how often should I "tock" the server
  heartBeatTockTimerSeconds: number = 10;
  // how often should I retry after connection loss?
  maxRetryAttempt: number = 5;
  // the retry should wait less long then the TockTimer, or calls may overlap
  retryWaitSeconds: number = this.heartBeatTockTimerSeconds / 2;
  // how many retry attempts did we have?
  currentRetryAttempt: number = 0;

  async delay(ms: number) {
    await new Promise(resolve => setTimeout(() => resolve(), ms));
  }

  startConnection = () => {
    this.currentRetryAttempt++;

    if (this.currentRetryAttempt > this.maxRetryAttempt) {
      console.log("Clearing registerHeartBeatTockTimer");
      clearInterval(this.heartBeatTockTimer);
      this.heartBeatTockTimer = null;
      throw "Retry attempts exceeded.";
    }

    let url = AppConsts.remoteServiceBaseUrl + "/notifications";

    this.connection = new signalR.HubConnectionBuilder().withUrl(url).build();

    this.connection.start().then(() => {
      this.currentRetryAttempt = 0;
      //this.connection.invoke('register');
      console.log("Should be connected!");
      this.addSignalRListener();
      this.registerHeartBeatTockTimer();
    })
      .catch(err => console.log('Error while starting connection: ', err));
  }

  addSignalRListener = () => {
    this.connection.on('send', (notification) => {
      console.log("Received notification:", notification);
    });

    this.connection.on(AppConsts.roleNotificationName, (notification) => {
      console.log("Role notification received:", notification);
      this.toast.fire({
        type: 'success',
        title: notification
      });
    });

    this.connection.on(AppConsts.userManagementNotificationName, (notification) => {
      console.log("Role notification received:", notification);
      this.toast.fire({
        type: 'success',
        title: notification
      });
    });

  }

  registerHeartBeatTockTimer() {
    // make sure we're registered only once
    if (this.heartBeatTockTimer != null)
      return;
    console.log("Registering registerHeartBeatTockTimer");
    if (this.connection !== null) {
      this.heartBeatTockTimer = setInterval(this.sendHeartBeatTock.bind(this), this.heartBeatTockTimerSeconds * 1000);
    }
    else {
      console.log("Connection didn't allow registry");
    }
  }

  sendHeartBeatTock() {
    if (this.connection == null) {
      console.log("Connection is null");
      return;
    }
    this.connection.invoke("HeartBeatTock").then(() => {
      console.log("Tock invoked...");
    })
      .catch(err => {
        console.log("HeartbeatTock Standard Error", err);
        this.delay.bind(this)(this.retryWaitSeconds * 1000).then(function () {
          console.log("executing attempt #" + this.currentRetryAttempt.toString());
          this.startConnection();
        });
        console.log("Current retry attempt: " + this.currentRetryAttempt);
      });
  }
}
