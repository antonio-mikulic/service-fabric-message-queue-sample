import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { finalize } from 'rxjs/operators';
import { MatDialog } from '@angular/material/dialog';
import { RoleManagementServiceProxy, RoleDto, BrokerServiceProxy } from '../../../shared/service-proxies/service-proxies';
import { CreateOrEditRoleDialogComponent } from './create-or-edit-role.component';
import { MatSort } from '@angular/material/sort';
import { AppConsts } from '../../../shared/AppConsts';

@Component({
  selector: 'roles',
  templateUrl: './roles.component.html',
  styleUrls: ['./roles.component.css']
})
export class RolesComponent implements OnInit {

  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;

  constructor(
    private _rolesServiceProxy: RoleManagementServiceProxy,
    private _brokerServiceProxy: BrokerServiceProxy,
    public dialog: MatDialog
  ) { }

  displayedColumns: string[] = ['name', 'actions'];
  dataSource: MatTableDataSource<RoleDto> = new MatTableDataSource<RoleDto>();
  isLoading: boolean = true;
  notificationsOn: boolean = false;

  ngOnInit() {
    this.getRoles();

    this.refreshNotificationSetting();
  }

  private getRoles(): void {
    this.isLoading = true;

    this._rolesServiceProxy.getAllRoles().subscribe((roles) => {
      this.dataSource = new MatTableDataSource<RoleDto>(roles);
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
      this.isLoading = false;

    }, finalize(() => this.isLoading = false));
  }

  refreshNotificationSetting() {
    this._brokerServiceProxy.isUserSubscribedToMessageType(AppConsts.roleNotificationName).subscribe((res) => {
      this.notificationsOn = res.succesfull;
    });
  }

  subToNotifications() {
    this._brokerServiceProxy.subscribeToMessageType(AppConsts.roleNotificationName).subscribe((res) => {
      this.refreshNotificationSetting();
    });
  }

  unsubFromNotifications() {
    this._brokerServiceProxy.unsubscribeFromMessageType(AppConsts.roleNotificationName).subscribe((res) => {
      this.refreshNotificationSetting();
    });
  }

  public delete(id: string): void {
    this._rolesServiceProxy.deleteRoleById(id).subscribe(() => {
      this.getRoles();
    });
  }

  public create(): void {
    this.edit(null);
  }

  public edit(id: string): void {
    let dialogRef = this.dialog.open(CreateOrEditRoleDialogComponent, {
      width: '750px',
      data: id
    });

    dialogRef.afterClosed().subscribe(result => {
      this.getRoles();
    });
  }

  public applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

}
