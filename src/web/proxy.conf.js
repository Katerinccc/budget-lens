// Dev-server proxy: the browser calls /api/... on the Angular origin and the request is forwarded
// to the BFF. Under Aspire the BFF address arrives as BFF_HTTP; standalone (`npm start`) it falls
// back to the BFF's default port. Angular never calls the .NET Api directly.
const target = process.env.BFF_HTTP || 'http://localhost:3000';

module.exports = {
  '/api': {
    target,
    secure: false,
    changeOrigin: true,
  },
};
