import { Component } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ApiService } from '@services/api.service';
import { NGXLogger } from 'ngx-logger';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'roles-component',
  templateUrl: './roles.component.html',
  styleUrls: ['./roles.component.less']
})
export class RolesComponent {
  public pageIsLoading: boolean = false;
  private subscriptions: Subject<any> = new Subject();
  public roles: any = [];
  public roleNameFilter: string = "";

  public roleForm: FormGroup;
  public roleModalIsVisible: boolean = false;

  public selectedRoleToDelete: string = '';
  public deleteModalIsVisible: boolean = false;

  public addCustomClaimForm: FormGroup;
  public selectedRoleIdToUpdate: string = '';
  public addCustomClaimModalIsVisible: boolean = false;
  public removeClaimModalIsVisible: boolean = false;
  public selectedClaimTypeToRemove: string = '';
  public selectedClaimValueToRemove: string = '';

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


  constructor(private apiService: ApiService,
    private formBuilder: FormBuilder,
    private logger: NGXLogger) {
    this.roleForm = this.formBuilder.group({
      id: null,
      roleName: [null, [Validators.required]]
    });
    this.addCustomClaimForm = this.formBuilder.group({
      claimType: [null, [Validators.required]],
      claimValue: [null, [Validators.required]]
    });
    this.getRoles();
  }

  //<get roles>
  getRoles() {
    this.pageIsLoading = true;
    this.apiService.getRoles(this.roleNameFilter)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (roles => {
          this.pageIsLoading = false;
          this.roles = roles;
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }
  clearSearchRoles() {
    this.roleNameFilter = '';
    this.getRoles();
  }
  //</get roles>

  //<role modal>
  showRoleFormModal(role?: any): void {
    if (role) {
      this.roleForm.patchValue({
        id: role.id,
        roleName: role.name
      });
    }
    this.roleModalIsVisible = true;
  }
  closeRoleModal() {
    this.roleModalIsVisible = false;
    this.roleForm.reset();
  }
  confirmRoleModal() {
    if (this.roleForm.value.id && this.roleForm.value.roleName != '') {
      this.updateRole();
    }
    else
      this.insertRole();
  }
  insertRole() {
    this.pageIsLoading = true;
    this.apiService.insertRole(this.roleForm.value.roleName)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeRoleModal();
          this.getRoles();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeRoleModal();
          this.logger.error(error);
          this.getRoles();
        })
      });
  }
  updateRole() {
    this.pageIsLoading = true;
    this.apiService.updateRole(this.roleForm.value.id, this.roleForm.value.roleName)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeRoleModal();
          this.getRoles();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeRoleModal();
          this.logger.error(error);
          this.getRoles();
        })
      });
  }
  //</role modal>

  //<delete role>
  showDeleteModal(roleIdToDelete: string): void {
    this.selectedRoleToDelete = roleIdToDelete;
    this.deleteModalIsVisible = true;
  }
  closeDeleteModal() {
    this.deleteModalIsVisible = false;
    this.selectedRoleToDelete = '';
  }
  confirmDeleteModal() {
    this.deleteRole();
  }
  deleteRole() {
    this.pageIsLoading = true;
    this.apiService.deleteRole(this.selectedRoleToDelete)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeDeleteModal();
          this.getRoles();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeDeleteModal();
          this.logger.error(error);
          this.getRoles();
        })
      });
  }
  //</delete role>

  //<add claim>
  showAddCustomClaimModal(roleIdToUpdate: string): void {
    this.selectedRoleIdToUpdate = roleIdToUpdate;
    this.addCustomClaimModalIsVisible = true;
  }
  closeAddCustomClaimModal() {
    this.addCustomClaimModalIsVisible = false;
    this.selectedRoleIdToUpdate = '';
    this.addCustomClaimForm.reset();
  }
  confirmAddCustomClaimModal() {
    if (this.addCustomClaimForm.value.claimType != '' && this.addCustomClaimForm.value.claimValue != '') {
      this.addCustomClaimToRole();
    }
  }
  addCustomClaimToRole() {
    this.pageIsLoading = true;
    this.apiService.addClaimToRole(this.selectedRoleIdToUpdate, this.addCustomClaimForm.value.claimType, this.addCustomClaimForm.value.claimValue)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeAddCustomClaimModal();
          this.getRoles();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeAddCustomClaimModal();
          this.logger.error(error);
          this.getRoles();
        })
      });
  }
  //</add claim>

  //<remove claim>
  showRemoveClaimModal(roleIdToUpdate: string, claimType: string, claimValue: string): void {
    this.selectedRoleIdToUpdate = roleIdToUpdate;
    this.selectedClaimTypeToRemove = claimType;
    this.selectedClaimValueToRemove = claimValue;
    this.removeClaimModalIsVisible = true;
  }
  closeRemoveClaimModal() {
    this.removeClaimModalIsVisible = false;
    this.selectedRoleIdToUpdate = '';
    this.selectedClaimTypeToRemove = '';
    this.selectedClaimValueToRemove = '';
  }
  confirmRemoveClaimModal() {
    this.removeClaimFromRole();
  }
  removeClaimFromRole() {
    this.pageIsLoading = true;
    this.apiService.removeClaimFromRole(this.selectedRoleIdToUpdate, this.selectedClaimTypeToRemove, this.selectedClaimValueToRemove)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeRemoveClaimModal();
          this.getRoles();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeRemoveClaimModal();
          this.logger.error(error);
          this.getRoles();
        })
      });
  }
  //</remove claim>

  //<existing claim>
  showExistingClaimsModal(roleIdToUpdate: string): void {
    this.selectedRoleIdToUpdate = roleIdToUpdate;
    this.existingClaimsModalIsVisible = true;
    this.getExistingClaims();
  }
  getExistingClaims() {
    this.pageIsLoading = true;
    this.apiService.getAvailableClaimsForRole(this.selectedRoleIdToUpdate)
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
    this.selectedRoleIdToUpdate = '';
    this.selectedClaimTypeToRemove = '';
    this.selectedClaimValueToRemove = '';
    this.existingClaimsFilter = '';
    this.getRoles();
  }
  addExistingClaimToRole(claimType: string, claimValue: string) {
    this.pageIsLoading = true;
    this.apiService.addClaimToRole(this.selectedRoleIdToUpdate, claimType, claimValue)
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
    this.selectedClaimTypeToRemove = claimType;
    this.selectedClaimValueToRemove = claimValue;
    this.removeExistingClaimModalIsVisible = true;
  }
  closeRemoveExistingClaimModal() {
    this.removeExistingClaimModalIsVisible = false;
  }
  confirmRemoveExistingClaimModal() {
    this.removeExistingClaimFromRole();
  }
  removeExistingClaimFromRole() {
    this.pageIsLoading = true;
    this.apiService.removeClaimFromRole(this.selectedRoleIdToUpdate, this.selectedClaimTypeToRemove, this.selectedClaimValueToRemove)
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

  ngOnDestroy(): void {
    this.subscriptions.next({});
    this.subscriptions.complete();
  }
}
