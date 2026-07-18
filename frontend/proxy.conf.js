// Development proxy only; production deployments must use validated TLS and environment routing.
module.exports = {
  "/api": {
    target:
      process.env["services__webapi__https__0"] ||
      process.env["services__webapi__http__0"] ||
      "http://localhost:5195",
    secure: false,
    pathRewrite: {
      "^/api": "",
    },
  },
};
