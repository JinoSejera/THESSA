using Microsoft.SemanticKernel;

namespace Extensions;

public static class KernelExtension
{
    public static void AddKernelServices(this IServiceCollection services)
    {
        services.AddSingleton<Kernel>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Kernel>>();
            try
            {
                var config = sp.GetRequiredService<IConfiguration>();
                IKernelBuilder builder = Kernel.CreateBuilder();
                builder.AddAzureOpenAIChatCompletion(
                    serviceId: "THESS",
                    deploymentName: config["AZURE_OPENAI_DEPLOYMENT_NAME"] ?? throw new ArgumentNullException("Azure OpenAI Deployment name not set."),
                    endpoint: config["AZURE_OPENAI_ENDPOINT"] ?? throw new ArgumentNullException("Azure OpenAI Endpoint not set."),
                    apiKey: config["AZURE_OPENAI_API_KEY"] ?? throw new ArgumentNullException("Azure OpenAI API Key not set."),
                    apiVersion: config["AZURE_OPENAI_API_VERSION"] ?? "2025-01-01-preview"
                    );

                var kernel = builder.Build();

                // Comment Generation Plugin
                var CommentGeneratorDirPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins", "CommentGenerator");

                // Load the Plugin from the Plugin Directory
                var PrReviewerFunc = kernel.ImportPluginFromPromptDirectory(CommentGeneratorDirPath);

                return kernel;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create the kernel.");
                throw;
            }
        });
    }
}
