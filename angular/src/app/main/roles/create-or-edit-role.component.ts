import { Component, Inject } from '@angular/core';
import { RoleDto, RoleManagementServiceProxy } from '../../../shared/service-proxies/service-proxies';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { finalize } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Component({
  selector: 'create-or-edit-role',
  templateUrl: './create-or-edit-role.component.html',
  styleUrls: ['./roles.component.css']
})
export class CreateOrEditRoleDialogComponent {

  public isLoading: boolean = true;
  public editMode: boolean;
  public role: RoleDto;

  constructor(
    public dialogRef: MatDialogRef<CreateOrEditRoleDialogComponent>,
    private _roleServiceProxy: RoleManagementServiceProxy,
    @Inject(MAT_DIALOG_DATA) public id: string) {
    this.loadRole();
  }

  loadRole() {
    this.role = new RoleDto();

    if (this.id == null) {
      this.isLoading = false;
      this.editMode = false;
    } else {
      this._roleServiceProxy.getSingleRoleById(this.id).subscribe((role) => {
        this.role.init(role);
        this.isLoading = false;
        this.editMode = true;
      }), finalize(() => this.isLoading = false);
    }
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  save() {
    let observable: Observable<RoleDto>;

    if (!this.editMode) {
      observable = this._roleServiceProxy.createRole(this.role);
    } else {
      observable = this._roleServiceProxy.updateRole(this.role);
    }

    observable.subscribe((res) => {
      this.dialogRef.close();
    });
  }
}
