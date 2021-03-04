import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { UserManagementServiceProxy, UserDto, BrokerServiceProxy } from '../../../shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';
import { MatDialog } from '@angular/material/dialog';
import { CreateOrEditUserDialogComponent } from './create-or-edit-user.component';
import { MatSort } from '@angular/material/sort';
import { AppConsts } from '../../../shared/AppConsts';

@Component({
  selector: 'users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {

  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;

  constructor(
    private _usersServiceProxy: UserManagementServiceProxy,
    private _brokerServiceProxy: BrokerServiceProxy,
    public dialog: MatDialog
  ) { }

  displayedColumns: string[] = ['userName', 'email', 'name', 'roles', 'actions'];
  dataSource: MatTableDataSource<UserDto> = new MatTableDataSource<UserDto>();
  isLoading: boolean = true;
  notificationsOn: boolean = false;

  ngOnInit() {
    this.getUsers();

    this.refreshNotificationSetting();

  }

  private getUsers(): void {
    // Start spinner
    this.isLoading = true; 

    // Fetch data
    this._usersServiceProxy.getAllUsers().subscribe((users: UserDto[]) => {
      // Received data is stored in users, and is stored as a list of UserDto

      // Refresh datasource data (items shown in table)
      this.dataSource = new MatTableDataSource<UserDto>(users);
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;

      // Remove spinner
      this.isLoading = false;

    }, finalize(() => this.isLoading = false) /* In case of an error or timeout remove spinner */);
  }

  public delete(id: string): void {
    this._usersServiceProxy.deleteUser(id).subscribe(() => {
      this.getUsers();
    });
  }

  public create(): void {
    this.edit(null);
  }

  public edit(id: string): void {
    let dialogRef = this.dialog.open(CreateOrEditUserDialogComponent, {
      width: '750px',
      data: id
    });

    dialogRef.afterClosed().subscribe(result => {
      this.getUsers();
    });
  }

  public applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  refreshNotificationSetting() {
    this._brokerServiceProxy.isUserSubscribedToMessageType(AppConsts.userManagementNotificationName).subscribe((res) => {
      this.notificationsOn = res.succesfull;
    });
  }

  subToNotifications() {
    this._brokerServiceProxy.subscribeToMessageType(AppConsts.userManagementNotificationName).subscribe((res) => {
      this.refreshNotificationSetting();
    });
  }

  unsubFromNotifications() {
    this._brokerServiceProxy.unsubscribeFromMessageType(AppConsts.userManagementNotificationName).subscribe((res) => {
      this.refreshNotificationSetting();
    });
  }


}
