import { Directive, ComponentRef, TemplateRef, ViewContainerRef, ComponentFactoryResolver, ComponentFactory, Input } from '@angular/core';
import { SpinnerComponent } from '../../ui/spinner/spinner.component';

@Directive({
  selector: '[appSpinner]'
})
export class SpinnerDirective {
  loadingFactory: ComponentFactory<SpinnerComponent>;
  loadingComponent: ComponentRef<SpinnerComponent>;
  
  @Input()
  set appSpinner(loading: boolean) {
    this.vcRef.clear();

    if (loading) {
      // create and embed an instance of the loading component
      this.loadingComponent = this.vcRef.createComponent(this.loadingFactory);
    }
    else {
      // embed the contents of the host template
      this.vcRef.createEmbeddedView(this.templateRef);
    }
  }

  constructor(private templateRef: TemplateRef<any>, private vcRef: ViewContainerRef, private componentFactoryResolver: ComponentFactoryResolver) {
    // Create resolver for loading component
    this.loadingFactory = this.componentFactoryResolver.resolveComponentFactory(SpinnerComponent);
  }

}