import { Component } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ApiService } from '@services/api.service';
import { NGXLogger } from 'ngx-logger';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'users-component',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.less']
})
export class UsersComponent {
  pageIsLoading: boolean = false;
  private subscriptions: Subject<any> = new Subject();
  public users: any = [];
  public usernameFilter: string = "";
  public emailFilter: string = "";

  //<insert user>
  public userForm: FormGroup;
  public userFormModalIsVisible: boolean = false;
  //</insert user>

  public selectedUserIdToDelete: string = '';
  public deleteUserModalIsVisible: boolean = false;

  //<add custom claim>
  public addCustomUserClaimForm: FormGroup;
  public selectedUserIdToUpdate: string = '';
  public addCustomUserClaimModalIsVisible: boolean = false;
  public removeCustomUserClaimModalIsVisible: boolean = false;
  public selectedUserClaimTypeToRemove: string = '';
  public selectedUserClaimValueToRemove: string = '';
  //</add custom claim>

  //<existing claim>
  public existingClaimsModalIsVisible: boolean = false;
  public existingClaims: any = [];
  public existingClaimsFiltered: any = [];
  public existingClaimsColumns: any = [
    {
      name: 'Added',
      sortOrder: null,
      sortFn: (a: any, b: any) => {
        return a.hasClaim - b.hasClaim;
      }
    },
    {
      name: 'Type',
      sortOrder: null,
      sortFn: (a: any, b: any) => {
        return a.claim.claimType.toLowerCase().localeCompare(b.claim.claimType.toLowerCase());
      }
    },
    {
      name: 'Value',
      sortOrder: null,
      sortFn: (a: any, b: any) => {
        return a.claim.claimValue.toLowerCase().localeCompare(b.claim.claimValue.toLowerCase());
      }
    }
  ];
  public existingClaimsFilter: string = '';
  public removeExistingClaimModalIsVisible: boolean = false;
  //</existing claim>

  //<remove role>
  public removeRoleModalIsVisible: boolean = false;
  //</remove role>

  //<existing role>
  public selectedRoleNameToRemove: string = '';
  public existingRolesModalIsVisible: boolean = false;
  public existingRoles: any = [];
  public existingRolesFiltered: any = [];
  public existingRolesColumns: any = [
    {
      name: 'Added',
      sortOrder: null,
      sortFn: (a: any, b: any) => {
        return a.hasRole - b.hasRole;
      }
    },
    {
      name: 'Name',
      sortOrder: null,
      sortFn: (a: any, b: any) => {
        return a.role.name.toLowerCase().localeCompare(b.role.name.toLowerCase());
      }
    }
  ];
  public existingRolesFilter: string = '';
  public removeExistingRoleModalIsVisible: boolean = false;
  //</existing role>

  constructor(private apiService: ApiService,
    private formBuilder: FormBuilder,
    private logger: NGXLogger) {
    this.userForm = this.formBuilder.group({
      id: null,
      userName: [null, [Validators.required]],
      email: [null, [Validators.required, Validators.pattern(/^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/)]],
      password: [null, [Validators.required, Validators.pattern(/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9])(?!.*\s).{8,15}$/)]],
      confirmPassword: [null, [Validators.required, Validators.pattern(/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9])(?!.*\s).{8,15}$/)]],
      editPassword: [null, [Validators.pattern(/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9])(?!.*\s).{8,15}$/)]],
      confirmEditPassword: [null, [Validators.pattern(/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9])(?!.*\s).{8,15}$/)]]
    });
    this.addCustomUserClaimForm = this.formBuilder.group({
      claimType: [null, [Validators.required]],
      claimValue: [null, [Validators.required]]
    });
    this.getUsers();
  }

  //<get users>
  getUsers() {
    this.pageIsLoading = true;
    this.apiService.getUsers(this.usernameFilter, this.emailFilter)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (users => {
          this.pageIsLoading = false;
          this.users = users;
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }
  clearSearchUsers() {
    this.usernameFilter = '';
    this.emailFilter = '';
    this.getUsers();
  }
  //</get users>

  //<user form>
  showUserFormModal(user?: any) {
    if (user) {
      this.userForm.patchValue({
        id: user.id,
        userName: user.userName,
        email: user.email,
        password: 'Tempo05rary?',
        confirmPassword: 'Tempo05rary?',
        editPassword: null,
        confirmEditPassword: null
      });
    }
    this.userFormModalIsVisible = true;
  }
  closeUserFormModal() {
    this.userFormModalIsVisible = false;
    this.userForm.reset();
  }
  confirmUserFormModal() {
    if (this.userForm.value.id && this.userForm.value.editPassword == this.userForm.value.confirmEditPassword) {
      this.updateUser();
    }
    else
      this.insertUser();
  }
  insertUser() {
    this.pageIsLoading = true;
    this.apiService.insertUser(this.userForm.value.userName,
      this.userForm.value.email,
      this.userForm.value.password,
      this.userForm.value.confirmPassword)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeUserFormModal();
          this.getUsers();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeUserFormModal();
          this.logger.error(error);
          this.getUsers();
        })
      });
  }
  updateUser() {
    this.pageIsLoading = true;
    this.apiService.updateUser(this.userForm.value.id,
      this.userForm.value.userName,
      this.userForm.value.email,
      this.userForm.value.editPassword)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeUserFormModal();
          this.getUsers();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeUserFormModal();
          this.logger.error(error);
          this.getUsers();
        })
      });
  }
  //</user form>

  //<delete user>
  showDeleteUserModal(userId: string) {
    this.selectedUserIdToDelete = userId;
    this.deleteUserModalIsVisible = true;
  }
  closeDeleteUserModal() {
    this.deleteUserModalIsVisible = false;
    this.selectedUserIdToDelete = '';
  }
  confirmDeleteUserModal() {
    this.deleteUser();
  }
  deleteUser() {
    this.pageIsLoading = true;
    this.apiService.deleteUser(this.selectedUserIdToDelete)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeDeleteUserModal();
          this.getUsers();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeDeleteUserModal();
          this.logger.error(error);
          this.getUsers();
        })
      });
  }
  //</delete user>

  //<add userclaim>
  showAddCustomUserClaimModal(userIdToUpdate: string): void {
    this.selectedUserIdToUpdate = userIdToUpdate;
    this.addCustomUserClaimModalIsVisible = true;
  }
  closeAddCustomUserClaimModal() {
    this.addCustomUserClaimModalIsVisible = false;
    this.selectedUserIdToUpdate = '';
    this.addCustomUserClaimForm.reset();
  }
  confirmAddCustomUserClaimModal() {
    if (this.addCustomUserClaimForm.value.claimType != '' && this.addCustomUserClaimForm.value.claimValue != '') {
      this.addCustomUserClaimToUser();
    }
  }
  addCustomUserClaimToUser() {
    this.pageIsLoading = true;
    this.apiService.addClaimToUser(this.selectedUserIdToUpdate, this.addCustomUserClaimForm.value.claimType, this.addCustomUserClaimForm.value.claimValue)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeAddCustomUserClaimModal();
          this.getUsers();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeAddCustomUserClaimModal();
          this.logger.error(error);
          this.getUsers();
        })
      });
  }
  //</add userclaim>

  //<remove user claim>
  showRemoveCustomUserClaimModal(userId: string, claimType: string, claimValue: string): void {
    this.selectedUserIdToUpdate = userId;
    this.selectedUserClaimTypeToRemove = claimType;
    this.selectedUserClaimValueToRemove = claimValue;
    this.removeCustomUserClaimModalIsVisible = true;
  }
  closeRemoveCustomUserClaimModal() {
    this.removeCustomUserClaimModalIsVisible = false;
    this.selectedUserIdToUpdate = '';
    this.selectedUserClaimTypeToRemove = '';
    this.selectedUserClaimValueToRemove = '';
  }
  confirmRemoveCustomUserClaimModal() {
    this.removeCustomUserClaimFromUser();
  }
  removeCustomUserClaimFromUser() {
    this.pageIsLoading = true;
    this.apiService.removeClaimFromUser(this.selectedUserIdToUpdate,
      this.selectedUserClaimTypeToRemove,
      this.selectedUserClaimValueToRemove)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeRemoveCustomUserClaimModal();
          this.getUsers();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeRemoveCustomUserClaimModal();
          this.logger.error(error);
          this.getUsers();
        })
      });
  }
  //</remove user claim>

  //<existing claim>
  showExistingClaimsModal(userIdToUpdate: string): void {
    this.selectedUserIdToUpdate = userIdToUpdate;
    this.existingClaimsModalIsVisible = true;
    this.getExistingClaims();
  }
  getExistingClaims() {
    this.pageIsLoading = true;
    this.apiService.getAvailableClaimsForUser(this.selectedUserIdToUpdate)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (claims => {
          this.pageIsLoading = false;
          this.existingClaims = claims.result;
          this.existingClaimsFiltered = claims.result;
          this.searchExistingClaims();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }
  closeExistingClaimsModal() {
    this.existingClaimsModalIsVisible = false;
    this.selectedUserIdToUpdate = '';
    this.selectedUserClaimTypeToRemove = '';
    this.selectedUserClaimValueToRemove = '';
    this.existingClaimsFilter = '';
    this.getUsers();
  }
  addExistingClaimToUser(claimType: string, claimValue: string) {
    this.pageIsLoading = true;
    this.apiService.addClaimToUser(this.selectedUserIdToUpdate, claimType, claimValue)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.getExistingClaims();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
          this.getExistingClaims();
        })
      });
  }
  searchExistingClaims() {
    this.existingClaimsFiltered = this.existingClaims.filter((item: any) => {
      return (item.claim.claimType.toLowerCase().indexOf(this.existingClaimsFilter.toLowerCase()) !== -1 ||
        item.claim.claimValue.toLowerCase().indexOf(this.existingClaimsFilter.toLowerCase()) !== -1);
    });
  }
  showRemoveExistingClaimModal(claimType: string, claimValue: string): void {
    this.selectedUserClaimTypeToRemove = claimType;
    this.selectedUserClaimValueToRemove = claimValue;
    this.removeExistingClaimModalIsVisible = true;
  }
  closeRemoveExistingClaimModal() {
    this.removeExistingClaimModalIsVisible = false;
  }
  confirmRemoveExistingClaimModal() {
    this.removeExistingClaimFromUser();
  }
  removeExistingClaimFromUser() {
    this.pageIsLoading = true;
    this.apiService.removeClaimFromUser(this.selectedUserIdToUpdate, this.selectedUserClaimTypeToRemove, this.selectedUserClaimValueToRemove)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeRemoveExistingClaimModal();
          this.getExistingClaims();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeRemoveExistingClaimModal();
          this.logger.error(error);
          this.getExistingClaims();
        })
      });
  }
  //</existing claim>

  //<remove role>
  showRemoveRoleModal(userId: string, roleName: string): void {
    this.selectedUserIdToUpdate = userId;
    this.selectedRoleNameToRemove = roleName;;
    this.removeRoleModalIsVisible = true;
  }
  closeRemoveRoleModal() {
    this.removeRoleModalIsVisible = false;
    this.selectedUserIdToUpdate = '';
    this.selectedRoleNameToRemove = '';
  }
  confirmRemoveRoleModal() {
    this.removeRoleFromUser();
  }
  removeRoleFromUser() {
    this.pageIsLoading = true;
    this.apiService.removeRoleFromUser(this.selectedUserIdToUpdate,
      this.selectedRoleNameToRemove)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeRemoveRoleModal();
          this.getUsers();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeRemoveRoleModal();
          this.logger.error(error);
          this.getUsers();
        })
      });
  }
  //</remove role>

  //<existing role>
  showExistingRolesModal(userIdToUpdate: string): void {
    this.selectedUserIdToUpdate = userIdToUpdate;
    this.existingRolesModalIsVisible = true;
    this.getExistingRoles();
  }
  getExistingRoles() {
    this.pageIsLoading = true;
    this.apiService.getAvailableRolesForUser(this.selectedUserIdToUpdate)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (roles => {
          this.pageIsLoading = false;
          this.existingRoles = roles.result;
          this.existingRolesFiltered = roles.result;
          this.searchExistingRoles();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }
  closeExistingRolesModal() {
    this.existingRolesModalIsVisible = false;
    this.selectedUserIdToUpdate = '';
    this.selectedRoleNameToRemove = '';
    this.existingRolesFilter = '';
    this.getUsers();
  }
  addExistingRoleToUser(roleName: string) {
    this.pageIsLoading = true;
    this.apiService.addRoleToUser(this.selectedUserIdToUpdate, roleName)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.getExistingRoles();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
          this.getExistingRoles();
        })
      });
  }
  searchExistingRoles() {
    this.existingRolesFiltered = this.existingRoles.filter((item: any) => {
      return (item.role.name.toLowerCase().indexOf(this.existingRolesFilter.toLowerCase()) !== -1);
    });
  }
  showRemoveExistingRoleModal(roleName: string): void {
    this.selectedRoleNameToRemove = roleName;
    this.removeExistingRoleModalIsVisible = true;
  }
  closeRemoveExistingRoleModal() {
    this.removeExistingRoleModalIsVisible = false;
  }
  confirmRemoveExistingRoleModal() {
    this.removeExistingRoleFromUser();
  }
  removeExistingRoleFromUser() {
    this.pageIsLoading = true;
    this.apiService.removeRoleFromUser(this.selectedUserIdToUpdate, this.selectedRoleNameToRemove)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeRemoveExistingRoleModal();
          this.getExistingRoles();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeRemoveExistingRoleModal();
          this.logger.error(error);
          this.getExistingRoles();
        })
      });
  }
  //</existing role>

  ngOnDestroy(): void {
    this.subscriptions.next({});
    this.subscriptions.complete();
  }
}
