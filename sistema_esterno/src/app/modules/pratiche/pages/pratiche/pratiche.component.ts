import { Component } from '@angular/core';
import { StateService } from '@services/state.service';
import { AuthService } from '@services/auth.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NGXLogger } from 'ngx-logger';
import { ApiService } from '@services/api.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzUploadFile } from 'ng-zorro-antd/upload';
import { __values } from 'tslib';

@Component({
  selector: 'pratiche-component',
  templateUrl: './pratiche.component.html',
  styleUrls: ['./pratiche.component.less']
})
export class PraticheComponent {
  user: any;
  selectedUsername: string = '';
  selectedPassword: string = '';
  pageIsLoading: boolean = false;
  private subscriptions: Subject<any> = new Subject();

  public pratiche: any = [];

  public praticaForm: FormGroup;
  public praticaModalIsVisible: boolean = false;

  public selectedPraticaToDelete: string = '';
  public deleteModalIsVisible: boolean = false;

  public selectedFileToUpload: NzUploadFile[] = [];
  public selectedFileToUploadAsBase64: string | null = null;

  public selectedPraticaHistoryIdToView: string = '';
  public praticaHistoryModalIsVisible: boolean = false;
  public praticaHistory: any = [];

  public selectedPraticaStatusIdToView: string = '';
  public praticaStatusModalIsVisible: boolean = false;
  public selectedPraticaStatusToApply: string = '';

  constructor(private apiService: ApiService,
    private stateService: StateService,
    private formBuilder: FormBuilder,
    private logger: NGXLogger) {
    this.praticaForm = this.formBuilder.group({
      id: null,
      selectedFileToUpload: [null, [Validators.required]],
      creatorName: [null, [Validators.required]],
      creatorLastName: [null, [Validators.required]],
      creatorCF: [null, [Validators.required]],
      creatorDateOfBirth: [null, [Validators.required]],
      status: [{ value: null, disabled: true }, [Validators.required, control =>
        control.value == 'Created'
          ? { invalidStatus: true }
          : null]],
      /*
      status: [{ value: null, disabled: true }, [Validators.required, control =>
        control.value === 'InProgress'
          ? { invalidStatus: true }
          : null]],
          */
      creationDate: [{ value: null, disabled: true }, [Validators.required]],
    });
    this.stateService.getUser()
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (user => {
          this.user = user;
        }),
        error: (error => {
          this.logger.error(error);
        })
      });
    this.getPratiche();
  }

  beforeUpload = (file: NzUploadFile): boolean => {
    this.selectedFileToUpload = [file];
    this.praticaForm.patchValue({ selectedFileToUpload: file });
    //this.praticaForm.get('selectedFileToUpload')?.markAsTouched();
    this.readAsBase64(file as unknown as File);
    return false // blocca l'upload automatico
  };

  private readAsBase64(file: File) {
    const reader = new FileReader();
    reader.onload = () => {
      this.selectedFileToUploadAsBase64 = (reader.result as string).split(',')[1];
    };
    reader.readAsDataURL(file);
  }

  onRemove = (): boolean => {
    this.selectedFileToUpload = [];
    this.selectedFileToUploadAsBase64 = null;
    this.praticaForm.patchValue({ selectedFileToUpload: null });
    //this.praticaForm.get('selectedFileToUpload')?.markAsTouched();
    return true;
  };

  //<get pratiche>
  getPratiche() {
    this.pageIsLoading = true;
    this.apiService.getPratiche()
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (pratiche => {
          this.pageIsLoading = false;
          this.pratiche = pratiche;
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }
  //</get pratiche>

  //<pratica modal>
  showPraticaFormModal(pratica?: any): void {
    console.log(pratica);
    if (pratica) {
      this.praticaForm.patchValue({
        id: pratica.id,
        creatorName: pratica.creatorName,
        creatorLastName: pratica.creatorLastName,
        creatorCF: pratica.creatorCF,
        creatorDateOfBirth: pratica.creatorDateOfBirth,
        contentBytes: pratica.contentBytes,
        status: pratica.status,
        creationDate: pratica.creationDate
      });
    }
    //devo aggiornarlo a mano sennò il form è invalido..
    if (pratica?.contentBytes) {
      this.selectedFileToUpload = [{
        uid: String(pratica.id),
        name: pratica.fileName || 'pratica.pdf',
        status: 'done'
      } as NzUploadFile];

      this.selectedFileToUploadAsBase64 = pratica.contentBytes;
      this.praticaForm.patchValue({ selectedFileToUpload: pratica.fileName || 'pratica.pdf' });
    }
    else if (pratica?.fileName && !pratica?.contentBytes) {
      this.selectedFileToUpload = [{
        uid: String(pratica.id),
        name: pratica.fileName || 'pratica.pdf',
        status: 'done'
      } as NzUploadFile];
      this.praticaForm.patchValue({ selectedFileToUpload: pratica.fileName || 'pratica.pdf' });
    }
    this.praticaModalIsVisible = true;
    console.log(this.praticaForm)
  }
  closePraticaModal() {
    this.praticaModalIsVisible = false;
    this.praticaForm.reset();
    this.selectedFileToUpload = [];
    this.selectedFileToUploadAsBase64 = null;
  }
  confirmPraticaModal() {
    if (this.selectedFileToUpload[0].name)
      if (this.praticaForm.value.id) {
        this.updatePratica();
      }
      else {
        this.insertPratica();
      }
  }
  insertPratica() {
    this.pageIsLoading = true;
    this.apiService.insertPratica(this.praticaForm.value.id, this.praticaForm.value.creatorName, this.praticaForm.value.creatorLastName,
      this.praticaForm.value.creatorCF, this.praticaForm.value.creatorDateOfBirth, this.praticaForm.value.status,
      this.selectedFileToUpload[0].name,
      this.selectedFileToUploadAsBase64)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closePraticaModal();
          this.getPratiche();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closePraticaModal();
          this.logger.error(error);
          this.getPratiche();
        })
      });
  }
  updatePratica() {
    this.pageIsLoading = true;
    this.apiService.updatePratica(this.praticaForm.value.id, this.praticaForm.value.creatorName, this.praticaForm.value.creatorLastName,
      this.praticaForm.value.creatorCF, this.praticaForm.value.creatorDateOfBirth, this.praticaForm.value.status,
      this.selectedFileToUpload[0].name,
      this.selectedFileToUploadAsBase64)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closePraticaModal();
          this.getPratiche();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closePraticaModal();
          this.logger.error(error);
          this.getPratiche();
        })
      });
  }
  //</pratica modal>

  //<delete pratica>
  showDeleteModal(praticaIdToDelete: string): void {
    this.selectedPraticaToDelete = praticaIdToDelete;
    this.deleteModalIsVisible = true;
  }
  closeDeleteModal() {
    this.deleteModalIsVisible = false;
    this.selectedPraticaToDelete = '';
  }
  confirmDeleteModal() {
    this.deletePratica();
  }
  deletePratica() {
    this.pageIsLoading = true;
    this.apiService.deletePratica(this.selectedPraticaToDelete)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closeDeleteModal();
          this.getPratiche();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closeDeleteModal();
          this.logger.error(error);
          this.getPratiche();
        })
      });
  }
  //</delete pratica>

  //<pratica history>
  showPraticaHistoryModal(praticaHistoryIdToView: string): void {
    this.selectedPraticaHistoryIdToView = praticaHistoryIdToView;
    this.praticaHistoryModalIsVisible = true;
    this.getPraticaHistory();
  }
  closePraticaHistoryModal() {
    this.praticaHistoryModalIsVisible = false;
    this.selectedPraticaHistoryIdToView = '';
  }
  getPraticaHistory() {
    this.pageIsLoading = true;
    this.apiService.getPraticaHistory(this.selectedPraticaHistoryIdToView)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (pratiche => {
          this.pageIsLoading = false;
          this.praticaHistory = pratiche.result;
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }
  //</pratica history>

  //<pratica status>
  showPraticaStatusModal(praticaStatusIdToView: string): void {
    this.selectedPraticaStatusIdToView = praticaStatusIdToView;
    this.praticaStatusModalIsVisible = true;
  }
  closePraticaStatusModal() {
    this.praticaStatusModalIsVisible = false;
    this.selectedPraticaStatusIdToView = '';
    this.selectedPraticaStatusToApply = '';
  }
  setSelectedPraticaStatus(value: any) {
    this.selectedPraticaStatusToApply = value;
  }
  confirmPraticaStatusModal() {
    this.changePraticaStatus();
  }
  changePraticaStatus() {
    this.pageIsLoading = true;
    this.apiService.changePraticaStatus(this.selectedPraticaStatusIdToView, this.selectedPraticaStatusToApply)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (response => {
          this.pageIsLoading = false;
          this.closePraticaStatusModal();
          this.getPratiche();
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.closePraticaStatusModal();
          this.logger.error(error);
          this.getPratiche();
        })
      });
  }
  //</pratica status>

  //<download pratica>
  downloadPratica(praticaId: string): void {
    this.pageIsLoading = true;
    this.apiService.downloadPratica(praticaId)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (pratica => {
          this.pageIsLoading = false;

          const byteChars = atob(pratica.result.contentBytes);
          const bytes = new Uint8Array(byteChars.length);
          for (let i = 0; i < byteChars.length; i++) {
            bytes[i] = byteChars.charCodeAt(i);
          }

          const blob = new Blob([bytes], { type: 'application/pdf' });
          const url = URL.createObjectURL(blob);

          const a = document.createElement('a');
          a.href = url;
          a.download = pratica.result.fileName;
          a.click();

          URL.revokeObjectURL(url);
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }
  downloadPraticaFromHistory(praticaId: string): void {
    this.pageIsLoading = true;
    this.apiService.downloadPraticaFromHistory(praticaId)
      .pipe(takeUntil(this.subscriptions))
      .subscribe({
        next: (pratica => {
          this.pageIsLoading = false;

          const byteChars = atob(pratica.result.contentBytes);
          const bytes = new Uint8Array(byteChars.length);
          for (let i = 0; i < byteChars.length; i++) {
            bytes[i] = byteChars.charCodeAt(i);
          }

          const blob = new Blob([bytes], { type: 'application/pdf' });
          const url = URL.createObjectURL(blob);

          const a = document.createElement('a');
          a.href = url;
          a.download = pratica.result.fileName;
          a.click();

          URL.revokeObjectURL(url);
        }),
        error: (error => {
          this.pageIsLoading = false;
          this.logger.error(error);
        })
      });
  }
  //</download pratica>

  ngOnDestroy(): void {
    this.subscriptions.next({});
    this.subscriptions.complete();
  }
}
