import { CommonModule } from '@angular/common';
import { Component, inject, OnDestroy, AfterViewInit, signal } from '@angular/core';
import * as L from 'leaflet';
import { BeneficiariosService } from '../beneficiarios/beneficiarios.service';
import { BeneficiarioMapa } from '../beneficiarios/models/beneficiario.model';

@Component({
  selector: 'app-mapa',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mapa.component.html',
  styleUrl: './mapa.component.css',
})
export class MapaComponent implements AfterViewInit, OnDestroy {
  private readonly service = inject(BeneficiariosService);
  private map?: L.Map;
  private markers = L.featureGroup();
  private carregamentoSolicitado = false;

  beneficiarios = signal<BeneficiarioMapa[]>([]);
  carregando = signal(true);
  erro = signal<string | null>(null);

  ngAfterViewInit(): void {
    this.inicializarMapa();
  }

  private inicializarMapa(): void {
    this.map = L.map('mapa-beneficiarios', {
      center: [-29.1634, -51.1797],
      zoom: 12,
      zoomControl: true,
    });

    const tiles = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
    });
    tiles.addTo(this.map);
    tiles.once('load', () => this.solicitarBeneficiariosAposMapa());
    setTimeout(() => this.solicitarBeneficiariosAposMapa(), 1500);

    this.markers.addTo(this.map);
  }

  private solicitarBeneficiariosAposMapa(): void {
    if (this.carregamentoSolicitado) return;
    this.carregamentoSolicitado = true;
    this.carregarBeneficiarios();
  }

  private carregarBeneficiarios(): void {
    this.carregando.set(true);
    this.erro.set(null);

    this.service.listarMapa().subscribe({
      next: beneficiarios => {
        this.beneficiarios.set(beneficiarios);
        this.adicionarMarcadores(beneficiarios);
        this.carregando.set(false);
      },
      error: () => {
        this.erro.set('Não foi possível carregar a localização dos beneficiários.');
        this.carregando.set(false);
      },
    });
  }

  private adicionarMarcadores(beneficiarios: BeneficiarioMapa[]): void {
    if (!this.map) return;

    this.markers.clearLayers();
    beneficiarios.forEach(beneficiario => {
      const marker = L.marker([beneficiario.latitude, beneficiario.longitude], {
        icon: L.divIcon({
          className: 'beneficiario-marker',
          html: [
            '<div style="position:relative;width:30px;height:38px;">',
            '<div style="position:absolute;left:3px;top:0;width:24px;height:24px;background:#0d9488;border:3px solid #fff;border-radius:50% 50% 50% 0;transform:rotate(-45deg);box-shadow:0 3px 8px rgba(15,23,42,.35);">',
            '<div style="position:absolute;width:8px;height:8px;left:5px;top:5px;background:#fff;border-radius:50%;"></div>',
            '</div>',
            '</div>',
          ].join(''),
          iconSize: [30, 38],
          iconAnchor: [15, 38],
          popupAnchor: [0, -32],
        }),
      });
      marker.bindPopup(`<strong>${this.escapeHtml(beneficiario.nome)}</strong><br>${this.escapeHtml(beneficiario.endereco)}`);
      this.markers.addLayer(marker);
    });

    if (beneficiarios.length > 0) {
      this.map.fitBounds(this.markers.getBounds(), { padding: [30, 30], maxZoom: 14 });
    }

    setTimeout(() => this.map?.invalidateSize(), 0);
  }

  private escapeHtml(value: string): string {
    const element = document.createElement('div');
    element.textContent = value;
    return element.innerHTML;
  }

  ngOnDestroy(): void {
    this.markers.clearLayers();
    this.map?.remove();
  }
}
