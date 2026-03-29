using Domain.AggregatesModel.ConnectorDefinitionAggregate;

namespace Infrastructure.EntityConfigurations;

public static class ConnectorSeedData
{
  public static async Task SeedConnectorDefinitionsAsync(ApiContext context)
  {
    // Verificar si ya existen definiciones de conectores
    if (await context.ConnectorDefinitions.AnyAsync())
    {
      return;
    }

    // Crear definición para Shopify
    var shopifyDefinition = ConnectorDefinition.Create(
        name: "Shopify",
        providerType: "Shopify",
        categoryType: "Ecommerce",
        requiresOAuth: true,
        logoUrl: new Uri("https://cdn.shopify.com/shopifycloud/brochure/assets/brand-assets/shopify-logo-primary-logo-456baa801ee66a0a435671082365958316831c9960c480451dd0330bcdae304f.svg"),
        description: "Conecta tu tienda Shopify para sincronizar productos, pedidos y clientes automáticamente.");

    // Crear definición para Bybit (API key; debe coincidir con GetByProviderTypeAsync("Bybit"))
    var bybitDefinition = ConnectorDefinition.Create(
        name: "Bybit",
        providerType: "Bybit",
        categoryType: "CryptoExchange",
        requiresOAuth: false,
        logoUrl: new Uri("https://s2.coinmarketcap.com/static/img/exchanges/64x64/271.png"),
        description: "Conecte su cuenta Bybit mediante API keys para consultar y sincronizar órdenes de futuros lineales (USDT perpetual), en mainnet o testnet.");

    context.ConnectorDefinitions.AddRange(shopifyDefinition, bybitDefinition);
    await context.SaveChangesAsync();
  }
}
