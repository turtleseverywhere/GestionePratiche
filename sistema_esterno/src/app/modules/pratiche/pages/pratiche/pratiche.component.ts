import { Component } from '@angular/core';
import { StateService } from '@services/state.service';
import { AuthService } from '@services/auth.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NGXLogger } from 'ngx-logger';
import { ApiService } from '@services/api.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzUploadFile } from 'ng-zorro-antd/upload';

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
      status: [{ value: null, disabled: true }, [Validators.required]],
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
    this.praticaForm.get('selectedFileToUpload')?.markAsTouched();
    this.readAsBase64(file as unknown as File);
    return false;                      // blocca l'upload automatico
  };

  private readAsBase64(file: File) {
    const reader = new FileReader();
    reader.onload = () => {
      // result = "data:application/pdf;base64,AAAA..."
      this.selectedFileToUploadAsBase64 = (reader.result as string).split(',')[1];
    };
    reader.readAsDataURL(file);
  }

  onRemove = (): boolean => {
    this.selectedFileToUpload = [];
    this.selectedFileToUploadAsBase64 = null;
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
    this.praticaModalIsVisible = true;
  }
  closePraticaModal() {
    this.praticaModalIsVisible = false;
    this.praticaForm.reset();
  }
  confirmPraticaModal() {


    const payload = {
      ...this.praticaForm.value,
      fileName: this.selectedFileToUpload[0].name,
      fileContent: this.selectedFileToUploadAsBase64,   // stringa base64
    };
    console.log(payload);


    if (this.praticaForm.value.id && this.praticaForm.value.creatorCF != '') {
      this.updatePratica();
    }
    else {
      console.log("insert");
      this.insertPratica();
    }
  }
  insertPratica() {
    this.pageIsLoading = true;
    this.apiService.insertPratica(this.praticaForm.value.creatorCF)
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
    this.apiService.updatePratica(this.praticaForm.value.id, this.praticaForm.value.creatorCF)
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
  showPraticaHistoryModal(praticaIdToDelete: string): void {

  }
  closePraticaHistoryModal() {

  }
  //</pratica history>

  //<download pratica>
  downloadPraticaPDF(praticaIdToDelete: string): void {

  }
  //</download pratica>

  ngOnDestroy(): void {
    this.subscriptions.next({});
    this.subscriptions.complete();
  }
}
