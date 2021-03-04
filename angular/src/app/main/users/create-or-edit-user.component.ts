import { Component, Inject } from '@angular/core';
import { UserManagementServiceProxy, UserDto, CreateOrUpdateUserDto, RoleManagementServiceProxy } from '../../../shared/service-proxies/service-proxies';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { finalize, findIndex } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { FormArray, FormControl } from '@angular/forms';

@Component({
  selector: 'create-or-edit-user',
  templateUrl: './create-or-edit-user.component.html',
  styleUrls: ['./users.component.css']
})
export class CreateOrEditUserDialogComponent {

  public isLoading: boolean = true;
  public editMode: boolean;
  public user: CreateOrUpdateUserDto;
  public confirmPassword: string = "";
  public roles: string[] = [];

  constructor(
    public dialogRef: MatDialogRef<CreateOrEditUserDialogComponent>,
    private _usersServiceProxy: UserManagementServiceProxy,
    private _rolesServiceProxy: RoleManagementServiceProxy,
    @Inject(MAT_DIALOG_DATA) public id: string) {
    this.loadUser();
    this.loadRoles();
  }

  private loadUser(): void {
    // Create a new dto used for filling properties, it shouldn't have any roles
    this.user = new CreateOrUpdateUserDto();
    this.user.roleNames = new Array<string>();

    if (this.id == null) {
      // If id is null that means new user should be created, so stop loading user
      this.isLoading = false;
      this.editMode = false;
    } else {
      // If id is set that means we have to get existing user from backend
      this._usersServiceProxy.getUserById(this.id).subscribe((user) => {
        /*
          We got user in user variable, and we will initilize our property called
          user with values of existing user. After that we should push all roles
          which exist on user to the dto we are using for editing
        */
        this.user.init(user);
        this.checkAssignedRoles();


        // Stop loading, declare that this dialog is used for editing
        this.isLoading = false;
        this.editMode = true;
      }), finalize(() => this.isLoading = false);
    }
  }

  private loadRoles(): void {
    this._rolesServiceProxy.getAllRoles().subscribe((res) => {
      this.roles = res.map(s => s.name);
    });
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  save() {
    if (!this.isValid()) return;

    let observable: Observable<UserDto>;

    if (!this.editMode) {
      observable = this._usersServiceProxy.createUser(this.user);
    } else {
      observable = this._usersServiceProxy.updateUser(this.user);
    }

    observable.subscribe((res) => {
      this.dialogRef.close();
    });
  }

  isValid(): boolean {
    if (this.editMode && (this.confirmPassword != this.user.password)) return false;

    return true;
  }

  onRoleChange(event) {
    if (event.checked) {
      this.user.roleNames.push(event.source.value);
    } else {
      const i = this.user.roleNames.findIndex(x => x === event.source.value);
      this.user.roleNames.splice(i, 0);
    }
  }

  checkAssignedRoles(){
    this.user.roleNames.forEach(roleName => {
      this.user.roleNames.push(roleName);
    });
  }

}
